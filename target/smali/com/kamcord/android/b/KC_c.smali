.class public final Lcom/kamcord/android/b/KC_c;
.super Lcom/kamcord/android/b/KC_e;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/b/KC_c$KC_a;
    }
.end annotation


# instance fields
.field private a:Lcom/kamcord/android/KamcordActivity;


# direct methods
.method public constructor <init>(Lcom/kamcord/android/KamcordActivity;)V
    .locals 0

    invoke-direct {p0}, Lcom/kamcord/android/b/KC_e;-><init>()V

    iput-object p1, p0, Lcom/kamcord/android/b/KC_c;->a:Lcom/kamcord/android/KamcordActivity;

    return-void
.end method

.method static synthetic a(Lcom/kamcord/android/b/KC_c;)Lcom/kamcord/android/KamcordActivity;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/b/KC_c;->a:Lcom/kamcord/android/KamcordActivity;

    return-object v0
.end method


# virtual methods
.method public final a()V
    .locals 2

    new-instance v0, Lcom/kamcord/android/b/KC_c$KC_a;

    invoke-direct {v0, p0}, Lcom/kamcord/android/b/KC_c$KC_a;-><init>(Lcom/kamcord/android/b/KC_c;)V

    const/4 v1, 0x0

    new-array v1, v1, [Ljava/lang/Void;

    invoke-virtual {v0, v1}, Lcom/kamcord/android/b/KC_c$KC_a;->execute([Ljava/lang/Object;)Landroid/os/AsyncTask;

    return-void
.end method

.method public final a(La/a/a/a/KC_e;Ljava/lang/String;Ljava/lang/String;)V
    .locals 0

    return-void
.end method

.method public final a(Landroid/content/Context;)V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/b/KC_c;->a:Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v0}, Lcom/kamcord/android/KamcordActivity;->getTabFragmentManager()Lcom/kamcord/android/KC_T;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/KC_P;

    invoke-direct {v1}, Lcom/kamcord/android/KC_P;-><init>()V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_T;->a(La/a/a/a/KC_e;)V

    return-void
.end method

.method public final b()Z
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v0

    const-string v1, "KamcordUsername"

    invoke-interface {v0, v1}, Landroid/content/SharedPreferences;->contains(Ljava/lang/String;)Z

    move-result v1

    if-eqz v1, :cond_0

    const-string v1, "KamcordEmail"

    invoke-interface {v0, v1}, Landroid/content/SharedPreferences;->contains(Ljava/lang/String;)Z

    move-result v1

    if-eqz v1, :cond_0

    const-string v1, "KamcordUserToken"

    invoke-interface {v0, v1}, Landroid/content/SharedPreferences;->contains(Ljava/lang/String;)Z

    move-result v0

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

    const-string v0, "Kamcord"

    return-object v0
.end method

.method public final e()I
    .locals 2

    const-string v0, "drawable"

    const-string v1, "kamcord"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    return v0
.end method

.method public final g()Ljava/lang/String;
    .locals 1

    const-string v0, "kamcord"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    return-object v0
.end method
