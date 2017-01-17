using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace EfTests
{
    namespace IdentifyingRelationship
    {
        public class Author
        {
            public Author()
            {

            }
            public int Id { get; set; }
            public string Name { get; set; }
            public virtual ICollection<Book> Books { get; set; } = new List<Book>();
        }

        public class Book
        {
            public Author Author { get; set; }
            public int Author_Id { get; set; }
            public int Id { get; set; }
            public string Title { get; set; }
        }

        public class BooksDbContext : DbContext
        {
            public BooksDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
            {
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Author>()
                    .HasMany(e => e.Books)
                    .WithRequired(e => e.Author)
                    .HasForeignKey(e => e.Author_Id)
                    ;
                modelBuilder.Entity<Book>()
                    .HasKey(e => new { e.Id, e.Author_Id })
                    .Property(e => e.Id)
                    .HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity);


                base.OnModelCreating(modelBuilder);
            }

            public IDbSet<Author> Authors { get; set; }
        }
        public class TestClass
        {
            public TestClass(ITestOutputHelper output)
            {
                this.output = output;
            }
            private const string ConnectionStringBooks = "server=MW-PC\\SQLEXPRESS;initial catalog=EfTests-Books;integrated security=true";
            private ITestOutputHelper output;
            [Fact]
            public void Can_delete_book()
            {
                Database.SetInitializer(new DropCreateDatabaseIfModelChanges<BooksDbContext>());

                using (var context = new BooksDbContext(ConnectionStringBooks))
                {
                    var transaction = context.Database.BeginTransaction();

                    context.Database.Log = output.WriteLine;

                    var bookToRemove = new Book() { Title = "Book 2" };
                    var author1 = new Author
                    {
                        Name = "Author 1",
                        Books =
                        {
                            new Book() { Title = "Book 1"},
                            bookToRemove,
                            new Book() { Title = "Book 3"},
                        }
                    };


                    context.Authors.Add(author1);

                    context.SaveChanges();

                    author1 = context.Authors.Find(author1.Id);

                    Assert.Equal(3, author1.Books.Count);
                    author1.Books.Remove(bookToRemove);
                    context.SaveChanges();
                    author1 = context.Authors.Find(author1.Id);
                    Assert.Equal(2, author1.Books.Count);

                    transaction.Rollback();

                }
            }
        }
    }


    namespace Definitions
    {
        class Definition
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public virtual UserContext UserContext { get; set; }

        }

        class UserContext
        {
            public int Id { get; set; }
            public string UserName { get; set; }

        }

        public class UserDbContext : System.Data.Entity.DbContext
        {
            public UserDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
            {
                this.Configuration.LazyLoadingEnabled = true;

            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Definition>()
                    .HasOptional(e => e.UserContext)
                    .WithOptionalPrincipal()
                    ;

                base.OnModelCreating(modelBuilder);
            }

        }

        public class MyClass
        {

            public MyClass(ITestOutputHelper output)
            {
                this.output = output;
            }
            private const string ConnectionStringDefinition = "server=MW-PC\\SQLEXPRESS;initial catalog=EfTests-Definition;integrated security=true";
            private ITestOutputHelper output;


            [Fact]
            private void Definition_one_to_one_fake()
            {
                Database.SetInitializer(new DropCreateDatabaseAlways<UserDbContext>());

                using (var context = new UserDbContext(ConnectionStringDefinition))
                {
                    var transaction = context.Database.BeginTransaction();

                    context.Database.Log = output.WriteLine;

                    var definition = new Definition();
                    var definition1 = new Definition { UserContext = new UserContext { UserName = "x" } };
                    var definition2 = new Definition { UserContext = new UserContext { UserName = "y" } };
                    Definition[] definitions =
                    {
                        definition,
                        definition1,
                        definition2
                    };

                    context.Set<Definition>().AddRange(definitions);

                    context.SaveChanges();

                    context.Set<Definition>().Where(d => d.Id == definition.Id)
                        //.Include(d => d.UserContext)
                        .Where(d => d.UserContext.UserName == "None" || d.UserContext == null).Single();
                    context.Set<Definition>().Where(d => d.Id == definition1.Id)
                        //.Include(d => d.UserContext)
                        .Where(d => d.UserContext.UserName == definition1.UserContext.UserName || d.UserContext == null).Single();
                    context.Set<Definition>().Where(d => d.Id == definition2.Id)
                        //.Include(d => d.UserContext)
                        .Where(d => d.UserContext.UserName == definition2.UserContext.UserName || d.UserContext == null).Single();

                    transaction.Commit();
                }


            }
        }

    }
    namespace Articles
    {
        public class ArticleDbContext : System.Data.Entity.DbContext
        {
            public ArticleDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
            {
                this.Configuration.LazyLoadingEnabled = true;
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ArticleRoot>()
                    .HasOptional(e => e.Article)
                    .WithOptionalPrincipal()
                    ;

                modelBuilder.Entity<ArticleRoot>()
                    .HasRequired(e => e.DraftArticle)
                    .WithRequiredPrincipal()
                    ;

                base.OnModelCreating(modelBuilder);
            }
        }



        public class ArticleRoot
        {
            public int Id { get; set; }
            public virtual Article Article { get; set; }
            public virtual DraftArticle DraftArticle { get; set; }


            public void PublishArticle()
            {
                Article = new Article();
            }
        }
        public class Article
        {
            public int Id { get; set; }

        }
        public class DraftArticle
        {
            public int Id { get; set; }

        }

        public class Class1
        {
            private const string ConnectionStringArticle = "server=MW-PC\\SQLEXPRESS;initial catalog=EfTests;integrated security=true";
            [Fact]
            private void Article_one_to_one()
            {
                Database.SetInitializer(new DropCreateDatabaseIfModelChanges<ArticleDbContext>());

                var root = new ArticleRoot() { DraftArticle = new DraftArticle() };


                var dbContext = new ArticleDbContext(ConnectionStringArticle);


                dbContext.Set<ArticleRoot>().Add(root);

                dbContext.SaveChanges();

                var rootId = root.Id;
                Assert.NotNull(root.DraftArticle);
                Assert.Null(root.Article);


                dbContext = new ArticleDbContext(ConnectionStringArticle);


                root = dbContext.Set<ArticleRoot>().Find(rootId);

                root.PublishArticle();

                dbContext.SaveChanges();


                dbContext = new ArticleDbContext(ConnectionStringArticle);


                root = dbContext.Set<ArticleRoot>().Find(rootId);

                Assert.NotNull(root.DraftArticle);
                Assert.NotNull(root.Article);

            }

        }

    }


}
