using Microsoft.AspNetCore.Identity;

namespace CaseMngmt.Server
{
    public class JapaneseIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError() =>
            new() { Code = nameof(DefaultError), Description = "不明なエラーが発生しました。" };

        public override IdentityError ConcurrencyFailure() =>
            new() { Code = nameof(ConcurrencyFailure), Description = "同時実行エラーが発生しました。オブジェクトが変更されています。" };

        public override IdentityError PasswordMismatch() =>
            new() { Code = nameof(PasswordMismatch), Description = "パスワードが一致しません。" };

        public override IdentityError InvalidToken() =>
            new() { Code = nameof(InvalidToken), Description = "無効なトークンです。" };

        public override IdentityError LoginAlreadyAssociated() =>
            new() { Code = nameof(LoginAlreadyAssociated), Description = "このログインはすでに別のユーザーに関連付けられています。" };

        public override IdentityError InvalidUserName(string? userName) =>
            new() { Code = nameof(InvalidUserName), Description = $"ユーザー名「{userName}」は無効です。英数字のみ使用できます。" };

        public override IdentityError InvalidEmail(string? email) =>
            new() { Code = nameof(InvalidEmail), Description = $"メールアドレス「{email}」は無効な形式です。" };

        public override IdentityError DuplicateUserName(string userName) =>
            new() { Code = nameof(DuplicateUserName), Description = $"ユーザー名「{userName}」はすでに使用されています。" };

        public override IdentityError DuplicateEmail(string email) =>
            new() { Code = nameof(DuplicateEmail), Description = $"メールアドレス「{email}」はすでに登録されています。" };

        public override IdentityError InvalidRoleName(string? role) =>
            new() { Code = nameof(InvalidRoleName), Description = $"ロール名「{role}」は無効です。" };

        public override IdentityError DuplicateRoleName(string role) =>
            new() { Code = nameof(DuplicateRoleName), Description = $"ロール名「{role}」はすでに存在します。" };

        public override IdentityError UserAlreadyHasPassword() =>
            new() { Code = nameof(UserAlreadyHasPassword), Description = "このユーザーはすでにパスワードが設定されています。" };

        public override IdentityError UserLockoutNotEnabled() =>
            new() { Code = nameof(UserLockoutNotEnabled), Description = "このユーザーに対してロックアウトが有効になっていません。" };

        public override IdentityError UserAlreadyInRole(string role) =>
            new() { Code = nameof(UserAlreadyInRole), Description = $"このユーザーはすでにロール「{role}」に属しています。" };

        public override IdentityError UserNotInRole(string role) =>
            new() { Code = nameof(UserNotInRole), Description = $"このユーザーはロール「{role}」に属していません。" };

        public override IdentityError PasswordTooShort(int length) =>
            new() { Code = nameof(PasswordTooShort), Description = $"パスワードは{length}文字以上にしてください。" };

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) =>
            new() { Code = nameof(PasswordRequiresUniqueChars), Description = $"パスワードには{uniqueChars}種類以上の異なる文字を含めてください。" };

        public override IdentityError PasswordRequiresNonAlphanumeric() =>
            new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "パスワードには記号（例: @、!、#）を1文字以上含めてください。" };

        public override IdentityError PasswordRequiresDigit() =>
            new() { Code = nameof(PasswordRequiresDigit), Description = "パスワードには数字（0〜9）を1文字以上含めてください。" };

        public override IdentityError PasswordRequiresLower() =>
            new() { Code = nameof(PasswordRequiresLower), Description = "パスワードには小文字（a〜z）を1文字以上含めてください。" };

        public override IdentityError PasswordRequiresUpper() =>
            new() { Code = nameof(PasswordRequiresUpper), Description = "パスワードには大文字（A〜Z）を1文字以上含めてください。" };
    }
}
