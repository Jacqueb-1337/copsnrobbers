.class public final Lcom/kamcord/android/b/KC_f;
.super Lcom/kamcord/android/b/KC_e;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/b/KC_f$KC_a;
    }
.end annotation


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, Lcom/kamcord/android/b/KC_e;-><init>()V

    return-void
.end method

.method private static h()Lcom/kamcord/a/a/d/KC_j;
    .locals 4

    const/4 v0, 0x0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v1

    const-string v2, "twitter_token"

    invoke-interface {v1, v2, v0}, Landroid/content/SharedPreferences;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    const-string v3, "twitter_secret"

    invoke-interface {v1, v3, v0}, Landroid/content/SharedPreferences;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    if-eqz v2, :cond_0

    if-eqz v1, :cond_0

    new-instance v0, Lcom/kamcord/a/a/d/KC_j;

    invoke-direct {v0, v2, v1}, Lcom/kamcord/a/a/d/KC_j;-><init>(Ljava/lang/String;Ljava/lang/String;)V

    :cond_0
    return-object v0
.end method

.method private static i()Lcom/kamcord/a/a/e/KC_c;
    .locals 2

    new-instance v0, Lcom/kamcord/a/a/a/KC_a;

    invoke-direct {v0}, Lcom/kamcord/a/a/a/KC_a;-><init>()V

    const-class v1, Lcom/kamcord/a/a/a/a/KC_f;

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->a(Ljava/lang/Class;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    const-string v1, "AMmZst034vuzxLIKhug1tw"

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->b(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    const-string v1, "JpQanvqL0EjpIVI4GrWhwxQ5ErRBFXfCaYeRgXUR20"

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->c(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    const-string v1, "http://localhost"

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/KC_a;->a(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/a/a/a/KC_a;->a()Lcom/kamcord/a/a/e/KC_c;

    move-result-object v0

    return-object v0
.end method


# virtual methods
.method public final a()V
    .locals 3

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v0

    invoke-interface {v0}, Landroid/content/SharedPreferences;->edit()Landroid/content/SharedPreferences$Editor;

    move-result-object v0

    const-string v1, "twitter_token"

    invoke-interface {v0, v1}, Landroid/content/SharedPreferences$Editor;->remove(Ljava/lang/String;)Landroid/content/SharedPreferences$Editor;

    move-result-object v0

    const-string v1, "twitter_secret"

    invoke-interface {v0, v1}, Landroid/content/SharedPreferences$Editor;->remove(Ljava/lang/String;)Landroid/content/SharedPreferences$Editor;

    move-result-object v0

    new-instance v1, Ljava/lang/StringBuilder;

    invoke-direct {v1}, Ljava/lang/StringBuilder;-><init>()V

    const-string v2, "TwitterUsername"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-interface {v0, v1}, Landroid/content/SharedPreferences$Editor;->remove(Ljava/lang/String;)Landroid/content/SharedPreferences$Editor;

    move-result-object v0

    invoke-interface {v0}, Landroid/content/SharedPreferences$Editor;->apply()V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getAuthCenter()Lcom/kamcord/android/KC_e;

    move-result-object v0

    const-string v1, "Twitter"

    const/4 v2, 0x0

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/android/KC_e;->a(Ljava/lang/String;Z)V

    return-void
.end method

.method public final a(La/a/a/a/KC_e;Ljava/lang/String;Ljava/lang/String;)V
    .locals 7

    const/4 v6, 0x0

    if-nez p3, :cond_0

    const-string p3, ""

    :cond_0
    invoke-static {}, Lcom/kamcord/android/b/KC_f;->i()Lcom/kamcord/a/a/e/KC_c;

    move-result-object v1

    if-eqz p3, :cond_1

    const-string v0, ""

    invoke-virtual {p3, v0}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_2

    :cond_1
    invoke-static {}, Lcom/kamcord/android/Kamcord;->getDefaultTweet()Ljava/lang/String;

    move-result-object p3

    :cond_2
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v2, "https://www.kamcord.com/v/"

    invoke-direct {v0, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    new-instance v3, Lcom/kamcord/a/a/d/KC_c;

    sget-object v0, Lcom/kamcord/a/a/d/KC_k;->b:Lcom/kamcord/a/a/d/KC_k;

    const-string v4, "https://api.twitter.com/1.1/statuses/update.json"

    invoke-direct {v3, v0, v4}, Lcom/kamcord/a/a/d/KC_c;-><init>(Lcom/kamcord/a/a/d/KC_k;Ljava/lang/String;)V

    invoke-virtual {v2}, Ljava/lang/String;->length()I

    move-result v0

    rsub-int/lit8 v0, v0, 0x64

    add-int/lit8 v0, v0, -0x1

    invoke-virtual {p3}, Ljava/lang/String;->length()I

    move-result v4

    if-le v0, v4, :cond_3

    invoke-virtual {p3}, Ljava/lang/String;->length()I

    move-result v0

    :cond_3
    invoke-virtual {p3, v6, v0}, Ljava/lang/String;->substring(II)Ljava/lang/String;

    move-result-object v0

    const-string v4, "status"

    new-instance v5, Ljava/lang/StringBuilder;

    invoke-direct {v5}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v5, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v5, " "

    invoke-virtual {v0, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v3, v4, v0}, Lcom/kamcord/a/a/d/KC_c;->c(Ljava/lang/String;Ljava/lang/String;)V

    invoke-static {}, Lcom/kamcord/android/b/KC_f;->h()Lcom/kamcord/a/a/d/KC_j;

    move-result-object v0

    if-nez v0, :cond_4

    sget-object v0, Lcom/kamcord/a/a/d/KC_b;->a:Lcom/kamcord/a/a/d/KC_j;

    :cond_4
    invoke-interface {v1, v0, v3}, Lcom/kamcord/a/a/e/KC_c;->a(Lcom/kamcord/a/a/d/KC_j;Lcom/kamcord/a/a/d/KC_c;)V

    new-instance v0, Lcom/kamcord/android/KC_C;

    invoke-direct {v0, v3, v1, p2}, Lcom/kamcord/android/KC_C;-><init>(Lcom/kamcord/a/a/d/KC_c;Lcom/kamcord/a/a/e/KC_c;Ljava/lang/String;)V

    new-array v1, v6, [Ljava/lang/Void;

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_C;->execute([Ljava/lang/Object;)Landroid/os/AsyncTask;

    return-void
.end method

.method public final a(Landroid/content/Context;)V
    .locals 4

    const/4 v3, 0x0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v0

    const-string v1, "twitter_token"

    invoke-interface {v0, v1}, Landroid/content/SharedPreferences;->contains(Ljava/lang/String;)Z

    move-result v1

    if-eqz v1, :cond_0

    const-string v1, "twitter_secret"

    invoke-interface {v0, v1}, Landroid/content/SharedPreferences;->contains(Ljava/lang/String;)Z

    move-result v0

    if-eqz v0, :cond_0

    :goto_0
    return-void

    :cond_0
    invoke-static {}, Lcom/kamcord/android/b/KC_f;->i()Lcom/kamcord/a/a/e/KC_c;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/b/KC_f$KC_a;

    invoke-direct {v1, p0, v3}, Lcom/kamcord/android/b/KC_f$KC_a;-><init>(Lcom/kamcord/android/b/KC_f;B)V

    const/4 v2, 0x1

    new-array v2, v2, [Lcom/kamcord/a/a/e/KC_c;

    aput-object v0, v2, v3

    invoke-virtual {v1, v2}, Lcom/kamcord/android/b/KC_f$KC_a;->execute([Ljava/lang/Object;)Landroid/os/AsyncTask;

    goto :goto_0
.end method

.method public final b()Z
    .locals 1

    invoke-static {}, Lcom/kamcord/android/b/KC_f;->h()Lcom/kamcord/a/a/d/KC_j;

    move-result-object v0

    if-eqz v0, :cond_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public final d()Ljava/lang/String;
    .locals 1

    const-string v0, "Twitter"

    return-object v0
.end method
