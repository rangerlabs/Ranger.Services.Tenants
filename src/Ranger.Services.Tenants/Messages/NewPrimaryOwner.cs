namespace Ranger.Services.Tenants {
    public class NewPrimaryOwner {
        public NewPrimaryOwner (string email, string firstName, string lastName, string password) {
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Password = password;

        }
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Password { get; }
    }
}