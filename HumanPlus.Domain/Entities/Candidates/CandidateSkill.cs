namespace HumanPlus.Domain.Entities.Candidates
{
    public class CandidateSkill
    {
        public int Id { get; set; }
        public int CandidateId { get; set; }
        public Candidate Candidate { get; set; } = null!;
        public int SkillId { get; set; }
        public MasterData.Skill Skill { get; set; } = null!;
    }
}
