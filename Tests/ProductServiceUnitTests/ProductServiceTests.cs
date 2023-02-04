using AutoMapper;
using Moq;
using lifeEcommerce.Data.UnitOfWork;
using lifeEcommerce.Services;
using lifeEcommerce.Models.Entities;
using lifeEcommerce.Models.Dtos.Category;

namespace Tests.CategoryServiceUnitTests
{
    public class CategoryServiceTests
    {
        #region Properties
        private Mock<IUnitOfWork> unitOfWorkMock;
        private Mock<IMapper> mapperMock;
        //private Mock<IControlSettingService> controlSettingServiceMock;
        //private Mock<ISharedService> sharedServiceMock;
        private CategoryService categoryService;

        #endregion

        #region Ctor
        public CategoryServiceTests()
        {
            unitOfWorkMock = new Mock<IUnitOfWork>();
            mapperMock = new Mock<IMapper>();

            categoryService = new CategoryService(unitOfWorkMock.Object, mapperMock.Object);
        }
        #endregion

        [Theory]
        [InlineData(1)]
        public async Task GetByIdAsync_GetsAVlidId_ReturnsAValidCategory(int categoryId)
        {
            #region Arrange
            unitOfWorkMock.Setup(x => x.Repository<Category>().GetByCondition(x => x.Id == categoryId)).Returns(ReturnCategories());
            mapperMock.Setup(x => x.Map<CategoryDto>(It.IsAny<Category>())).Returns(ReturnCategoryDto());
            #endregion

            #region Act
            var getCategoryByIdAsync = await categoryService.GetCategory(categoryId);
            #endregion

            #region Assert
            Assert.NotNull(getCategoryByIdAsync);
            Assert.Equal(categoryId, getCategoryByIdAsync.Id);
            #endregion
        }

        public CategoryDto ReturnCategoryDto()
        {
            var data = new CategoryDto
            {
                Id = 1,
                Name = "Test Name",
                DisplayOrder = 1,
                CreatedDateTime = DateTime.Now
            };

            return data;
        }

        public IQueryable<Category> ReturnCategories()
        {
            var list = new List<Category>
            {
                new Category
                {
                    Id = 1,
                    Name = "Test Name",
                    DisplayOrder = 1,
                    CreatedDateTime = DateTime.Now
                }
            };
            var queryableCategory = list.AsQueryable<Category>();
            return queryableCategory;
        }
    }
}
