using Project1.Core.Entities;

namespace Project1.Core.Systems.Conversations;

abstract record class ConvoSubject;
record class ConvoSubject_Entity(Entity Subject) : ConvoSubject;
record class ConvoSubject_Concept(Def Concept) : ConvoSubject;
record struct ConvoSubjectNew(EntityRefId Subject, Def Concept) { }
record struct ConvoDeltas(float TalkerNeed, float ListenerNeed, int TalkerXp, int TalkerRel, int ListenerRel) { }
record struct ConvoInputs(int TalkerSkill, float TalkerManner, float TalkerSelflessness, float ListenerResilience) { }