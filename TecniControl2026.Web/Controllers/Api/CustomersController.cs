using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Authorization;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.DTOs.Customer;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ApiController
    {
        private readonly ICustomersService _customersService;

        public CustomersController(ICustomersService customersService)
        {
            _customersService = customersService;
        }

        [HttpGet]
        [HasPermission(PermissionsCatalog.Customers.Show)]
        public async Task<IActionResult> Get([FromQuery] PaginationRequest request)
        {
            Response<PaginationResponse<CustomerDTO>> response = await _customersService.GetPaginationAsync(request);
            return ControllerBasicValidation(response);
        }

        [HttpPost]
        [HasPermission(PermissionsCatalog.Customers.Create)]
        public async Task<IActionResult> Post([FromBody] CreateCustomerDTO dto)
        {
            Response<CreateCustomerDTO> response = await _customersService.CreateAsync(dto);
            return ControllerBasicValidation(response);
        }

        [HttpPut]
        [HasPermission(PermissionsCatalog.Customers.Update)]
        public async Task<IActionResult> Put([FromBody] UpdateCustomerDTO dto)
        {
            Response<CustomerDTO> response = await _customersService.UpdateAsync(dto);
            return ControllerBasicValidation(response);
        }

        [HttpDelete("{id}")]
        [HasPermission(PermissionsCatalog.Customers.Update)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            Response<object> response = await _customersService.ToggleAsync(new ToggleCustomerStatusDTO
            {
                CustomerId = id,
                IsActive = false
            });

            return ControllerBasicValidation(response);
        }
    }
}