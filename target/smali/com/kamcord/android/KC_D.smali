.class public final Lcom/kamcord/android/KC_D;
.super Landroid/os/AsyncTask;


# annotations
.annotation system Ldalvik/annotation/Signature;
    value = {
        "Landroid/os/AsyncTask",
        "<",
        "Ljava/lang/Void;",
        "Ljava/lang/Void;",
        "Ljava/lang/Boolean;",
        ">;"
    }
.end annotation


# instance fields
.field private a:Lcom/kamcord/a/a/d/KC_c;

.field private b:Lcom/kamcord/a/a/e/KC_c;

.field private c:Lcom/kamcord/a/a/d/KC_h;


# direct methods
.method public constructor <init>(Lcom/kamcord/a/a/d/KC_c;Lcom/kamcord/a/a/e/KC_c;)V
    .locals 0

    invoke-direct {p0}, Landroid/os/AsyncTask;-><init>()V

    iput-object p1, p0, Lcom/kamcord/android/KC_D;->a:Lcom/kamcord/a/a/d/KC_c;

    iput-object p2, p0, Lcom/kamcord/android/KC_D;->b:Lcom/kamcord/a/a/e/KC_c;

    return-void
.end method

.method private varargs a()Ljava/lang/Boolean;
    .locals 4

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/KC_D;->a:Lcom/kamcord/a/a/d/KC_c;

    invoke-virtual {v0}, Lcom/kamcord/a/a/d/KC_c;->b()Lcom/kamcord/a/a/d/KC_h;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_D;->c:Lcom/kamcord/a/a/d/KC_h;
    :try_end_0
    .catch Ljava/lang/NullPointerException; {:try_start_0 .. :try_end_0} :catch_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_1

    :goto_0
    iget-object v0, p0, Lcom/kamcord/android/KC_D;->c:Lcom/kamcord/a/a/d/KC_h;

    invoke-virtual {v0}, Lcom/kamcord/a/a/d/KC_h;->a()Z

    move-result v0

    if-nez v0, :cond_1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getAuthCenter()Lcom/kamcord/android/KC_e;

    move-result-object v0

    iget-object v1, p0, Lcom/kamcord/android/KC_D;->b:Lcom/kamcord/a/a/e/KC_c;

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_e;->a(Lcom/kamcord/a/a/e/KC_c;)V

    :cond_0
    const/4 v0, 0x0

    invoke-static {v0}, Ljava/lang/Boolean;->valueOf(Z)Ljava/lang/Boolean;

    move-result-object v0

    :goto_1
    return-object v0

    :catch_0
    move-exception v0

    const-string v1, "Null request!"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/NullPointerException;->printStackTrace()V

    goto :goto_0

    :catch_1
    move-exception v0

    invoke-virtual {v0}, Ljava/lang/Exception;->printStackTrace()V

    goto :goto_0

    :cond_1
    new-instance v0, Ljava/lang/StringBuilder;

    invoke-direct {v0}, Ljava/lang/StringBuilder;-><init>()V

    iget-object v1, p0, Lcom/kamcord/android/KC_D;->b:Lcom/kamcord/a/a/e/KC_c;

    invoke-interface {v1}, Lcom/kamcord/a/a/e/KC_c;->b()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, "Username"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    iget-object v1, p0, Lcom/kamcord/android/KC_D;->b:Lcom/kamcord/a/a/e/KC_c;

    iget-object v2, p0, Lcom/kamcord/android/KC_D;->c:Lcom/kamcord/a/a/d/KC_h;

    invoke-interface {v1, v2}, Lcom/kamcord/a/a/e/KC_c;->a(Lcom/kamcord/a/a/d/KC_h;)Ljava/lang/String;

    move-result-object v1

    if-eqz v0, :cond_0

    if-eqz v1, :cond_0

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Setting "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, " to "

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, " in shared preferences."

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v2

    invoke-interface {v2}, Landroid/content/SharedPreferences;->edit()Landroid/content/SharedPreferences$Editor;

    move-result-object v2

    invoke-interface {v2, v0, v1}, Landroid/content/SharedPreferences$Editor;->putString(Ljava/lang/String;Ljava/lang/String;)Landroid/content/SharedPreferences$Editor;

    move-result-object v0

    invoke-interface {v0}, Landroid/content/SharedPreferences$Editor;->apply()V

    const/4 v0, 0x1

    invoke-static {v0}, Ljava/lang/Boolean;->valueOf(Z)Ljava/lang/Boolean;

    move-result-object v0

    goto :goto_1
.end method


# virtual methods
.method protected final synthetic doInBackground([Ljava/lang/Object;)Ljava/lang/Object;
    .locals 1

    invoke-direct {p0}, Lcom/kamcord/android/KC_D;->a()Ljava/lang/Boolean;

    move-result-object v0

    return-object v0
.end method

.method protected final synthetic onPostExecute(Ljava/lang/Object;)V
    .locals 2

    check-cast p1, Ljava/lang/Boolean;

    invoke-virtual {p1}, Ljava/lang/Boolean;->booleanValue()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getAuthCenter()Lcom/kamcord/android/KC_e;

    move-result-object v0

    iget-object v1, p0, Lcom/kamcord/android/KC_D;->b:Lcom/kamcord/a/a/e/KC_c;

    invoke-interface {v1}, Lcom/kamcord/a/a/e/KC_c;->b()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_e;->a(Ljava/lang/String;)V

    :cond_0
    return-void
.end method
