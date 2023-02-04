using lifeEcommerce.Data.UnitOfWork;
using lifeEcommerce.Helpers;
using lifeEcommerce.Models.Entities;
using LifeHangfireJobs.Dtos;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace LifeHangfireJobs.Services
{
    public class LifeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;

        public LifeService(IUnitOfWork unitOfWork, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
        }

        public async Task NotifyAdmin()
        {
            var newlyCreatedOrdersIds = await _unitOfWork.Repository<OrderData>()
                                                        .GetByCondition(x => x.OrderStatus == StaticDetails.Created)
                                                        .Select(x => x.OrderId)
                                                        .ToListAsync();

            foreach (var orderId in newlyCreatedOrdersIds)
            {
                await _emailSender.SendEmailAsync("albion.b@gjirafa.com", "New order to check", $"New order created: {orderId}");
            }

        }

        public async Task GetMetrics()
        {
            var orders = await _unitOfWork.Repository<OrderDetails>()
                                                                    .GetAll()
                                                                    .Include(x => x.Product)
                                                                    .Include(x => x.OrderData)
                                                                    .ToListAsync();

            var maxValue = orders.Max(x => x.Price);
            var minValue = orders.Min(x => x.Price);

            // if we want additional data
            //var mostExpensiveOrder = orders.First(x => x.Price == maxValue);
            //var cheapestOrder = orders.First(x => x.Price == minValue);

            var productsGroup = orders.GroupBy(x => x.ProductId)
                .ToDictionary(x => x.Key, y => y.Select(z => z.Count).Sum());

            var mostSoldProduct = productsGroup.MaxBy(kvp => kvp.Value).Key;

            var bestDay = orders.Select(x => DateOnly.FromDateTime(x.OrderData.OrderDate))
                                        .GroupBy(i => i)
                                        .OrderByDescending(grp => grp.Count())
                                        .FirstOrDefault()!.Key;

            var model = new Metrics()
            {
                MostSoldProduct = mostSoldProduct,
                MostExpensiveOrder = (double)maxValue,
                CheapestOrder = (double)minValue,
                BestDay = bestDay
            };
        }
    }
}
