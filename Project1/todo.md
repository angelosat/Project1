# todo 2025
## immediate
- decide wether itempreferencemanager will only be about job related tools or items in general
- make maps, slots, and containers, implement iowner with an add and remove method, adding to a container, removes it from the last one
  - not 100% necessary if i do entity.map?.despawn(entity), entity.slot?.set(null), entity.container?.remove(entity) at each entry point
- fix moodlets being checked only in specific context (dont check the noroom moodlet for non town members)
  - [Open MoodletDef.cs](Components/Mood/MoodletDef.cs)
- fix "visitor" visiting for the first time when not actually the first time 
- use getbuffer() instead of toarray() for outgoing packet streams [Open Stream.cs](Network/Stream.cs)
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