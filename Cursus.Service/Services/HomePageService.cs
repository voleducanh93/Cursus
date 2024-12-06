using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Service.Services
{
    public class HomePageService : IHomePageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public HomePageService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<HomePage> GetHomePageAsync()
        {
            var homePage = await _unitOfWork.HomePageRepository.GetAllAsync();
            if (homePage == null) throw new KeyNotFoundException("Home Page infomation not found");
            var homePageInfo = homePage
                        .OrderByDescending(hp => hp.LastUpdatedDate)
                        .FirstOrDefault();
            return homePageInfo;
        }

        public async Task<HomePageDTO> UpdateHomePageAsync(int id, HomePageDTO homePageDto)
        {
            var homePage = await _unitOfWork.HomePageRepository.GetAsync(hp => hp.Id==id);
            if (homePage == null) throw new KeyNotFoundException("Home Page infomation not found");

            _mapper.Map(homePageDto, homePage);
            _unitOfWork.HomePageRepository.UpdateAsync(homePage);
            await _unitOfWork.SaveChanges();
            return _mapper.Map<HomePageDTO>(homePage);
        }

    }
}
