using AutoMapper;
using ClosedXML;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Service.Services
{
    public class TermPolicyService : ITermPolicyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TermPolicyService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Term>> GetTermPolicyAsync()
        {
            var termPolicies = await _unitOfWork.TermPolicyRepository.GetAllAsync();
            if (termPolicies == null || !termPolicies.Any()) throw new KeyNotFoundException("Term Policy not found");
            return termPolicies;
        }
        public async Task<TermPolicyDTO> CreateTermPolicyAsync(TermPolicyDTO termPolicyDTO)
        {
            var termPolicy = _mapper.Map<Term>(termPolicyDTO);

            // Add the new term policy to the repository
            await _unitOfWork.TermPolicyRepository.AddAsync(termPolicy);
            await _unitOfWork.SaveChanges();

            // Return the newly created DTO
            return _mapper.Map<TermPolicyDTO>(termPolicy);
        }

        public async Task DeleteTermPolicyAsync(int id)
        {
            var termPolicy = await _unitOfWork.TermPolicyRepository.GetAsync(t=>t.Id==id);
            if (termPolicy == null ) throw new KeyNotFoundException("Term Policy not found");

            _unitOfWork.TermPolicyRepository.DeleteAsync(termPolicy);
            await _unitOfWork.SaveChanges();
        }

        public async Task<TermPolicyDTO> UpdateTermPolicyAsync(int id, TermPolicyDTO termPolicyDTO)
        {
            var termPolicy = await _unitOfWork.TermPolicyRepository.GetAsync(t => t.Id==id);
            if (termPolicy == null) throw new KeyNotFoundException("Term Policy not found");

            // Map the updated properties from the DTO to the entity
            _mapper.Map(termPolicyDTO, termPolicy);

            _unitOfWork.TermPolicyRepository.UpdateAsync(termPolicy);
            await _unitOfWork.SaveChanges();

            return _mapper.Map<TermPolicyDTO>(termPolicy);
        }
    }
}
