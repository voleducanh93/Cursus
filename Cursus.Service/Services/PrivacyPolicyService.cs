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
    public class PrivacyPolicyService : IPrivacyPolicyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PrivacyPolicyService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PrivacyPolicy>> GetPrivacyPolicyAsync()
        {
            var privacyPolicies = await _unitOfWork.PrivacyPolicyRepository.GetAllAsync();
            if (privacyPolicies == null || !privacyPolicies.Any()) throw new KeyNotFoundException("Privacy Policy not found");
            return privacyPolicies;
        }

        public async Task<PrivacyPolicyDTO> CreatePrivacyPolicyAsync(PrivacyPolicyDTO privacyPolicyDTO)
        {
            var privacyPolicy = _mapper.Map<PrivacyPolicy>(privacyPolicyDTO);

            await _unitOfWork.PrivacyPolicyRepository.AddAsync(privacyPolicy);
            await _unitOfWork.SaveChanges();

            return _mapper.Map<PrivacyPolicyDTO>(privacyPolicy);
        }

        public async Task DeletePrivacyPolicyAsync(int id)
        {
            var privacyPolicy = await _unitOfWork.PrivacyPolicyRepository.GetAsync(p => p.Id == id);
            if (privacyPolicy == null)
                throw new KeyNotFoundException("Privacy Policy not found");

            _unitOfWork.PrivacyPolicyRepository.DeleteAsync(privacyPolicy);
            await _unitOfWork.SaveChanges();
        }

        public async Task<PrivacyPolicyDTO> UpdatePrivacyPolicyAsync(int id, PrivacyPolicyDTO privacyPolicyDTO)
        {
            var privacyPolicy = await _unitOfWork.PrivacyPolicyRepository.GetAsync(p => p.Id == id);
            if (privacyPolicy == null)
                throw new KeyNotFoundException("Privacy Policy not found");

            _mapper.Map(privacyPolicyDTO, privacyPolicy);

            _unitOfWork.PrivacyPolicyRepository.UpdateAsync(privacyPolicy);
            await _unitOfWork.SaveChanges();

            return _mapper.Map<PrivacyPolicyDTO>(privacyPolicy);
        }
    }
}
