
using CRM_mvc.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace CRM_mvc.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Answer>()
                .HasMany(e => e.ReturnActions)
                .WithMany(e => e.Answers)
                .UsingEntity<AnswerReturnAction>();
            
            builder.Entity<Chat>()
                .HasMany(e => e.Sessions)
                .WithMany(e => e.Chats)
                .UsingEntity<ChatSessions>();

            builder.Entity<ApplicationUser>()
                .HasMany(e => e.Extentions)
                .WithMany(e => e.Users)
                .UsingEntity<ExtensionUsers>();

            builder.Entity<Call>()
                .HasMany(e => e.Answers)
                .WithMany(e => e.Calls)
                .UsingEntity<CallAnswers>();
            
            base.OnModelCreating(builder);
        }
        
        
        
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Call> Calls { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ChatChannel> ChatChannels { get; set; }
        public DbSet<ReturnAction> ReturnActions { get; set; }
        public DbSet<AnswerReturnAction> AnswerReturnActions { get; set; }
        public DbSet<CustomerVerification> CustomerVerifications { get; set; }
        public DbSet<MessageAttachment> MessageAttachments { get; set; }
        public DbSet<CallChannel> CallChannels { get; set; }
        public DbSet<CallType> CallTypes { get; set; }
        public DbSet<Extension> Extensions { get; set; }
        public DbSet<ExtensionUsers> ExtensionUsers { get; set; }
        public DbSet<CallAnswers> CallAnswers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<ProjectType> ProjectTypes { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<ChatSessions> ChatSessions { get; set; }
    }
}