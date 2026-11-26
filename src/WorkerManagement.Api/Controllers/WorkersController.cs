using Microsoft.AspNetCore.Mvc;
using WorkerManagement.Api.Contracts;
using WorkerManagement.BusinessLogic;

namespace WorkerManagement.Api.Controllers;

[ApiController]
[Route("workers")]
public sealed class WorkersController(IWorkerService workerService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WorkerModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WorkerModel>>> GetAll(
        CancellationToken cancellationToken)
    {
        var workers = await workerService.GetAllAsync(cancellationToken);
        return Ok(workers.Select(Map).ToArray());
    }

    [HttpGet("{workerId}")]
    [ProducesResponseType(typeof(WorkerModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkerModel>> GetById(
        string workerId,
        CancellationToken cancellationToken)
    {
        var worker = await workerService.GetByIdAsync(workerId, cancellationToken);
        return Ok(Map(worker));
    }

    [HttpPost]
    [ProducesResponseType(typeof(WorkerModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WorkerModel>> Create(
        CreateWorkerModel model,
        CancellationToken cancellationToken)
    {
        var worker = await workerService.CreateAsync(
            new CreateWorkerRequest(
                model.WorkerId,
                model.Name,
                model.Email,
                model.Department,
                model.Designation),
            cancellationToken);

        var response = Map(worker);
        return CreatedAtAction(nameof(GetById), new { workerId = response.WorkerId }, response);
    }

    [HttpPut("{workerId}")]
    [ProducesResponseType(typeof(WorkerModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkerModel>> Update(
        string workerId,
        UpdateWorkerModel model,
        CancellationToken cancellationToken)
    {
        var worker = await workerService.UpdateAsync(
            workerId,
            new UpdateWorkerRequest(
                model.Name,
                model.Email,
                model.Department,
                model.Designation),
            cancellationToken);

        return Ok(Map(worker));
    }

    [HttpDelete("{workerId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        string workerId,
        CancellationToken cancellationToken)
    {
        await workerService.DeleteAsync(workerId, cancellationToken);
        return NoContent();
    }

    private static WorkerModel Map(WorkerResponse worker) => new(
        worker.WorkerId,
        worker.Name,
        worker.Email,
        worker.Department,
        worker.Designation);
}
