using GameStore.API.Data;
using GameStore.API.Dtos;
using GameStore.API.Entities;
using GameStore.API.Mappings;
using Microsoft.EntityFrameworkCore;

namespace GameStore.API.Endpoints;

public static class GamesEndpoints
{
    const string GETGAME_ENDPOINT = "GetGame";
    public static RouteGroupBuilder MapGamesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("games")
            .WithParameterValidation();

        //GET /games
        group.MapGet("/", async (GameStoreContext dbContext) => 
           await dbContext.Games
                    .Include(game => game.Genre)
                    .Select(game => game.ToGameSummaryDto())
                    .AsNoTracking().ToListAsync());

        //GET /games/1  
        group.MapGet("/{id}", async (int id, GameStoreContext dbContext) => 
        {
            Game? game = await dbContext.Games.FindAsync(id);
            return game is null ?
                 Results.NotFound() : Results.Ok(game.ToGameDetailsDto());
        })
        .WithName(GETGAME_ENDPOINT);

        //POST /games
        group.MapPost("/", async (CreateGameDto newGame, GameStoreContext dbContext) =>
        {
            Game game = newGame.ToEntity();

            dbContext.Games.Add(game);
            await dbContext.SaveChangesAsync();
            
            return Results.CreatedAtRoute(
                GETGAME_ENDPOINT,
                new {id = game.Id},
                game.ToGameDetailsDto());
        });

        //PUT /games/1
        group.MapPut("/{id}", async (int id, UpdateGameDto updatedGame, GameStoreContext dbContext) =>
        {
            Game? game = await dbContext.Games.FindAsync(id);

            if(game is null) return Results.NotFound();
            
            dbContext.Entry(game)
                .CurrentValues
                .SetValues(updatedGame.ToEntity(id));

            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (int id, GameStoreContext dbContext) =>
        {
            await dbContext.Games
                     .Where(game => game.Id == id)
                     .ExecuteDeleteAsync();

            return Results.NoContent();
        });

        return group;
    }
}
