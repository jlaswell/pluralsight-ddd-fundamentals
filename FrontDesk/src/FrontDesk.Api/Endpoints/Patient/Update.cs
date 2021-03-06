﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using AutoMapper;
using BlazorShared.Models.Patient;
using FrontDesk.Core.Aggregates;
using FrontDesk.Core.Specifications;
using Microsoft.AspNetCore.Mvc;
using PluralsightDdd.SharedKernel.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace FrontDesk.Api.PatientEndpoints
{
  public class Update : BaseAsyncEndpoint<UpdatePatientRequest, UpdatePatientResponse>
  {
    private readonly IRepository _repository;
    private readonly IMapper _mapper;

    public Update(IRepository repository, IMapper mapper)
    {
      _repository = repository;
      _mapper = mapper;
    }

    [HttpPut("api/patients")]
    [SwaggerOperation(
        Summary = "Updates a Patient",
        Description = "Updates a Patient",
        OperationId = "patients.update",
        Tags = new[] { "PatientEndpoints" })
    ]
    public override async Task<ActionResult<UpdatePatientResponse>> HandleAsync(UpdatePatientRequest request, CancellationToken cancellationToken)
    {
      var response = new UpdatePatientResponse(request.CorrelationId());

      var spec = new ClientByIdIncludePatientsSpecification(request.ClientId);
      var client = await _repository.GetAsync<Client, int>(spec);
      if (client == null) return NotFound();

      var patientToUpdate = client.Patients.FirstOrDefault(p => p.Id == request.PatientId);
      if (patientToUpdate == null) return NotFound();

      patientToUpdate.UpdateName(request.Name);

      await _repository.UpdateAsync<Client, int>(client);

      var dto = _mapper.Map<PatientDto>(patientToUpdate);
      response.Patient = dto;

      return Ok(response);
    }
  }
}
