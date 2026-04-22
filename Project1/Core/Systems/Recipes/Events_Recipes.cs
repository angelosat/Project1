using Project1.Core.Entities.Actors;
using Project1.Framework.Events;

namespace Project1.Core.Systems.Recipes;

internal record struct ActorRecipeMasteryEvent(Actor Actor, RecipeKnowledge Knowledge) : IEventPayload;
