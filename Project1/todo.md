# todo 2025
## immediate
- fix moodlets being checked only in specific context (dont check the noroom moodlet for noncitizens)
  - [Open MoodletDef.cs](Components/Mood/MoodletDef.cs)
	- Condition = a => a.IsCitizen && a.AssignedRoom == null 
	- fix IsCitizen (rename to IsTownMember) so that it doesn't check the actor's *nullable* Map field 
	- decouple town from map, probably i need assignable unique town ids, so that the actor knows its part of a town even if the map/town isn't loaded
## networking
## gui
- xml driven
## ai
### town members
- they try to keep the tools associated with their active labors in their inventory. when a labor is toggled off, they drop the tool or go and deposit it in a stockpile if it's not required by any other labors. or in a container designated as their own storage for their owned items
## content
### npc visitors (players)
- npcs can die and lose their equipment
  - the lost equipment still exists in the world, maybe the enemy that killed them steals it
- npc equipment loses durability, when they break they come back to town to buy new
- npc chance to visit the town increases as:
  - their equipment loses durability or breaks, so they need to repair or buy new
  - their inventory fills up so they need to come back to sell
### off-map npc exploration
- create enemy entities. entities in general shouldn't necessarily have a graphics componentnecessarily, or a sprite/animation associated with it, because they can also simply live off-map to act as npc encounters