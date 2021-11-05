namespace TMF.SmartAssistant.FuncApps.Application.Model
{
    internal class OrganizationMember
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode() + Email.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as OrganizationMember);
        }
        public bool Equals(OrganizationMember obj)
        {
            return obj != null && obj.Id == this.Id;
        }
    }
}
