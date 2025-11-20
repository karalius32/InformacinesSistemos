using NpgsqlTypes;

namespace InformacinesSistemos.Models.Enums
{
    public enum UserRole
    {
        [PgName("administrator")]
        Administrator,

        [PgName("reader")]
        Reader,

        [PgName("librarian")]
        Librarian
    }

    public enum SubscriptionLevel
    {
        [PgName("bronze")]
        Bronze,

        [PgName("silver")]
        Silver,

        [PgName("gold")]
        Gold
    }

    public enum AuthorRole
    {
        [PgName("author")]
        Author,

        [PgName("coauthor")]
        Coauthor,

        [PgName("editor")]
        Editor,

        [PgName("translator")]
        Translator
    }
}
