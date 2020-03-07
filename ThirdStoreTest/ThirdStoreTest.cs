using System;
using Xunit;
using Rhino.Mocks;
using ThirdStoreData;
using ThirdStoreCommon.Models.Image;
using ThirdStoreCommon.Infrastructure;
using ThirdStoreBusiness.Image;
using ThirdStoreCommon;
using System.Collections.Generic;
using System.Linq;

namespace ThirdStoreTest
{
    public class UnitTest1
    {
        [Fact]
        public void Insert_Image_Was_Called()
        {
            //Assert.Equal(4, Add(2, 2));
            var _imageRepository = MockRepository.GenerateMock<IRepository<D_Image>>();
            var _workContext= MockRepository.GenerateMock<IWorkContext>();

            var _imgService = new ImageService(_imageRepository,_workContext);
            var newImg = new D_Image();
            var imageID = 1233;
            var url = $"{ThirdStoreConfig.Instance.ThirdStoreDomainURL}/ThirdStoreImagesPath/{string.Format("{0}.{1}", imageID.ToString("0000000"), "jpg")}";
            _imageRepository.Stub(r => r.Insert(Arg<D_Image>.Is.Anything));
            //_imageRepository.Expect(r => r.Table).Return(new List<D_Image>() { new D_Image()}.AsQueryable());

            _imgService.InsertImage(newImg);

            //Assert.True(_imageRepository.Table.Count()==1);


            _imageRepository.AssertWasCalled(r => r.Insert(Arg<D_Image>.Is.Anything));

            //_imgService.VerifyAllExpectations();
            //Assert.NotEqual(url, _imgService.GetImageURL(imageID));
        }

        [Fact]
        public void ImageService_Insert_Image()
        {
            var _imageRepository = MockRepository.GenerateMock<IRepository<D_Image>>();
            var _workContext = MockRepository.GenerateMock<IWorkContext>();

            var _imgService = new ImageService(_imageRepository, _workContext);
            var insertImg = new D_Image();
            Assert.Equal(insertImg, _imgService.InsertImage(insertImg));
        }

        //[Fact]
        //public void SyncInventory_

        private int Add(int x, int y)
        {
            return x + y;
        }
    }
}
