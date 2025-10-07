using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[ApiVersion(2.0)]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;
    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService; 
    }

    [Authorize(AuthConstants.TrustedMemberPolicyName)]
    [HttpPost(ApiEndPoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody]CreateMovieRequest request, CancellationToken token)
    {
        var movie = request.MaptoMovie();
        await _movieService.CreateAsync(movie, token);
        return CreatedAtAction(nameof(GetV1), new { idOrSlug = movie.Id }, movie);
    }

    [MapToApiVersion(1.0)]
    [Authorize]
    [HttpGet(ApiEndPoints.Movies.Get)]
    public async Task<IActionResult> GetV1([FromRoute] string idOrSlug, CancellationToken token)
    {
        var userId = HttpContext.GetUserId();
        var movie = Guid.TryParse(idOrSlug, out var id)
            ? await _movieService.GetByIdAsync(id, userId, token)
            : await _movieService.GetBySlugAsync(idOrSlug, userId, token);
        if (movie is null)
        {
            return NotFound();
        }

        var response = movie.MapToResponse();
        return Ok(response);
    }

    [MapToApiVersion(2.0)]
    [Authorize]
    [HttpGet(ApiEndPoints.Movies.Get)]
    public async Task<IActionResult> GetV2([FromRoute] string idOrSlug, CancellationToken token)
    {
        var userId = HttpContext.GetUserId();
        var movie = Guid.TryParse(idOrSlug, out var id)
            ? await _movieService.GetByIdAsync(id, userId, token)
            : await _movieService.GetBySlugAsync(idOrSlug, userId, token);
        if (movie is null)
        {
            return NotFound();
        }

        var response = movie.MapToResponse();
        return Ok(response);
    }


    [Authorize]
    [HttpGet(ApiEndPoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllMoviesRequest request, CancellationToken token)
     {
        var userId = HttpContext.GetUserId();
        var options = request.MapToOptions()
            .WithUser(userId);
        var movies = await _movieService.GetAllAsync(options, token);
        var movieCount = await _movieService.GetCountAsync(options.Title, options.YearOfRelease, token);

        var moviesResponse = movies.MapToResponse(request.Page, request.PageSize, movieCount);
        return Ok(moviesResponse);
    }
     
    [Authorize(AuthConstants.TrustedMemberPolicyName)]
    [HttpPut(ApiEndPoints.Movies.Update)]
    public async Task<IActionResult> Update([FromRoute]Guid id, [FromBody]UpdateMovieRequest request, CancellationToken token)
    {
        var movie = request.MaptoMovie(id);
        var userId = HttpContext.GetUserId();
        var updatedMovie = await _movieService.UpdateAsync(movie, userId, token);
        if (updatedMovie is null)
        {
            return NotFound();
        }
        var response = updatedMovie.MapToResponse();
        return Ok(response);
    }

    [Authorize(AuthConstants.AdminUserPolicyName)]
    [HttpDelete(ApiEndPoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute]Guid id, CancellationToken token)
    {
        var deleted = await _movieService.DeleteByIdAsync(id, token);
        if (!deleted)
        {
            return NotFound();
        }

        return Ok();
    }
}

