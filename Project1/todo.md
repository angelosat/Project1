# todo 2025
## immediate
- unknown block selection not revealing material in selection panel
- change entity materials from per-bone to a materialcomp, which defines whether the item has material at all, etc
- influence system for visitors to help with town tasks
- clarify auxiliary/additional targets to reserve in plans
- lean more into crafting commitments in crafting manager
- recipe / spell mastery / crafting order filter by actor
- move retrievefrominventory logic from smartequip to consumable planner
- front-back of shop cash registers/counters and workstations
- customer queues
- HUGE BUG: if smartequip has cause an actor to equip an item while his preexisting plan involved carrying another item, then the preexisting plan has to be aborted 
- - otherwise the actor will attempt to resume the plan with the wrong carried item
- head animation for conversations
- health going negative while losing it offmap
- make mood signed
- forsale flag: maybe keep forsale stockpiles that they simply automatically mark each item that lands within them as for sale?
- crafting: commit crafting contract not just before returning a final craft plan, but when the actor determines feasibility of a crafting order. to prevent other actors from trying to fulfil the same order inbeteen planner ticks
- crafting: figure out when to uncommit interrupted crafting commitments. maybe attach the craftingorder or the commitment to every plan returned by plannercrafting
- and let craftingmanager listen to actorassigneplan events, and uncommit whenever a plan doesnt have the commited order attached
- offmapactivity meet other npc and gain social need
- dev/debug increase/decrease skill levels
- recalculate path on cell invalidation
- buffs + buff/blessing town service
- restaurant town service
- bank town service (adventurers deposit their leftover coins before going out for adventure)
- make conversation requests expire (currently by patience)
- 
- create primary raw material processing workstations
- separate pathing behavior from interaction behavior. probably dont need behaviorexecuteplan anymore, just assume that all behaviors are a path to an interaction
- repair: create a blockrepaircomp for workstations with repair capability that holds repair charges similar to how 
blockfuelcomp holds fuel, and make it rechargable by scraps, which are byproducts from raw material refinement
(logs => planks + wood scraps, ore => ingots + metal scraps, etc)
- plans: maybe rename targeta and targetb to interactiontarget and pathingtarget, and accept a list of extra targets to reserve optionally
- sending empty snapshots: if i send empty snapshots, then entities "jump" to their next position on the client. if i dont send empty snapshots, entities "jump" to the position they last rested at when they start moving again on clients
- durability-repair
- harvesting-cooking
- list<control> controls make it private
- change camera.rotation to int instead of double
- influence resource for having visitors help with town jobs? or only when a certain reputation thershold has been reached?
- change camera zoom to only be >=1 , and for <1 make it actually reduce the rendertarget bounds (for 1-1 pixel mapping)
- change regionnodes to be the actual cell the actors stand in, instead of the cell below (WHAT WAS I THINKING)
- cleanup interactions/blocks
- construction designations: instead of drawing on the chunk's meshes, let construction manager have its own mesh. this way it will only serve as a layer/interface to select cells, intead of actually placing dummy "designation" blocks
- lumberjacking: two distinct designations: 1)chop down explicitly: forces actors to clear trees immediately, 2)chop down only when wood stocks lower than min set limit
- crafting: clear workstation surface from irrelevant items before working
- make maps, slots, and containers, implement iowner with an add and remove method, adding to a container, removes it from the last one
  - not 100% necessary if i do entity.map?.despawn(entity), entity.slot?.set(null), entity.container?.remove(entity) at each entry point
- fix moodlets being checked only in specific context (dont check the noroom moodlet for non town members)
  - [Open MoodletDef.cs](Components/Mood/MoodletDef.cs)
- fix "visitor" visiting for the first time when not actually the first time 
- use getbuffer() instead of toarray() for outgoing packet streams [Open Stream.cs](Network/Stream.cs)
- order town memebers to transfer an inventory item to another member
  - this item is marked for transfer and in the first opportunity a relevant taskgiver will push this task to the owner which will then path to the target actor and perform the interaction
  - or (easier way maybe) the item can be assigned an owner through its ui, which is also accessible by clicking on it within an actor inventory, and the its owner can be set there.
	- the mentioned task giver periodically scans the actor inventory for items that have a different owner, and creates task to maybe to give it to them, or move it to a stockpile
  - instead of single actor ownership / or on top of, maybe create "shared" items, and toggle which ai actors are permitted to use it
## networking
## gui
- xml driven
## ai
### town members
- create a item like/dislike registry . use it to add a temporary dislike for items that are dropped from the inventory as a result of player input, to prevent the actor from immediately picking it up back again
## content
### npc visitors (players)
- npcs can die and lose their equipment
  - the lost equipment still exists in the world, maybe the enemy that killed them steals it
  - lost equipment are added to a pool that other adventurers have a chance to come across
- npc equipment loses durability, when they break they come back to town to buy new
- npc chance to visit the town increases as:
  - their equipment loses durability or breaks, so they need to repair or buy new
  - their inventory fills up so they need to come back to sell
- visitor will help with town tasks if they like the town enough
  - or you can create quests that reward visitors from helping with things like digging and chopping?
  #### quests
  - make a quest posting board object that the visitor will go accept quests from. when a player creates a quest, in order for it to be available to visitors, the quest reward budget has to be commited. for example if the player creates a quest (bring bag 10 rat tailes: reward 1 gold) then the player has the option to commit some budget to it, for example, 100 gold. then the quest can be available to visitor as long as the budget hsan't been fully comsumed by successful quest returns
### off-map npc exploration
- create enemy entities. entities in general shouldn't necessarily have a graphics componentnecessarily, or a sprite/animation associated with it, because they can also simply live off-map to act as npc encounters