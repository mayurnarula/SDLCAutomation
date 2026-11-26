using Microsoft.AspNetCore.Mvc;
using WorkerManagement.Api.Contracts;
using WorkerManagement.BusinessLogic.Models;
using WorkerManagement.BusinessLogic.Services;

namespace WorkerManagement.Api.Controllers;

[ApiController]
[Route("workers")]
[Produces("application/json")]
public sealed class WorkersController(IWorkerService workerService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<WorkerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<WorkerResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var workers = await workerService.GetAllAsync(cancellationToken);
        return Ok(workers.Select(ToResponse).ToArray());
    }

    [HttpGet("{workerId}")]
    [ProducesResponseType(typeof(WorkerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkerResponse>> GetById(string workerId, CancellationToken cancellationToken)
    {
        var worker = await workerService.GetByIdAsync(workerId, cancellationToken);
        return Ok(ToResponse(worker));
    }

    [HttpPost]
    [ProducesResponseType(typeof(WorkerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WorkerResponse>> Create(
        CreateWorkerRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return ValidationProblem(new Dictionary<string, string[]>
            {
                ["request"] = ["A request body is required."]
            });
        }

        var worker = await workerService.CreateAsync(
            new WorkerInput(
                request.WorkerId,
                request.Name,
                request.Email,
                request.Department,
                request.Designation),
            cancellationToken);

        var response = ToResponse(worker);
        return CreatedAtAction(nameof(GetById), new { workerId = worker.WorkerId }, response);
    }

    [HttpPut("{workerId}")]
    [ProducesResponseType(typeof(WorkerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkerResponse>> Update(
        string workerId,
        UpdateWorkerRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return ValidationProblem(new Dictionary<string, string[]>
            {
                ["request"] = ["A request body is required."]
            });
        }

        var worker = await workerService.UpdateAsync(
            workerId,
            new WorkerUpdateInput(
                request.Name,
                request.Email,
                request.Department,
                request.Designation),
            cancellationToken);

        return Ok(ToResponse(worker));
    }

    [HttpDelete("{workerId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string workerId, CancellationToken cancellationToken)
    {
        await workerService.DeleteAsync(workerId, cancellationToken);
        return NoContent();
    }

    private static WorkerResponse ToResponse(Worker worker) => new(
        worker.WorkerId,
        worker.Name,
        worker.Email,
        worker.Department,
        worker.Designation);
}
