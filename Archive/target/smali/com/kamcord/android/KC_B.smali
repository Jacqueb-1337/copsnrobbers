.class public final Lcom/kamcord/android/KC_B;
.super Landroid/os/AsyncTask;


# annotations
.annotation system Ldalvik/annotation/Signature;
    value = {
        "Landroid/os/AsyncTask",
        "<",
        "Ljava/lang/Void;",
        "Ljava/lang/Void;",
        "Ljava/lang/Void;",
        ">;"
    }
.end annotation


# instance fields
.field private a:Lcom/kamcord/a/a/e/KC_c;

.field private b:Lcom/kamcord/a/a/d/KC_l;

.field private c:Lcom/kamcord/a/a/d/KC_j;


# direct methods
.method public constructor <init>(Lcom/kamcord/a/a/e/KC_c;Lcom/kamcord/a/a/d/KC_l;)V
    .locals 0

    invoke-direct {p0}, Landroid/os/AsyncTask;-><init>()V

    iput-object p1, p0, Lcom/kamcord/android/KC_B;->a:Lcom/kamcord/a/a/e/KC_c;

    iput-object p2, p0, Lcom/kamcord/android/KC_B;->b:Lcom/kamcord/a/a/d/KC_l;

    return-void
.end method

.method private varargs a()Ljava/lang/Void;
    .locals 5

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/KC_B;->a:Lcom/kamcord/a/a/e/KC_c;

    iget-object v1, p0, Lcom/kamcord/android/KC_B;->b:Lcom/kamcord/a/a/d/KC_l;

    invoke-interface {v0, v1}, Lcom/kamcord/a/a/e/KC_c;->a(Lcom/kamcord/a/a/d/KC_l;)Lcom/kamcord/a/a/d/KC_j;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_B;->c:Lcom/kamcord/a/a/d/KC_j;
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    :goto_0
    iget-object v0, p0, Lcom/kamcord/android/KC_B;->c:Lcom/kamcord/a/a/d/KC_j;

    if-eqz v0, :cond_3

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getAuthCenter()Lcom/kamcord/android/KC_e;

    iget-object v0, p0, Lcom/kamcord/android/KC_B;->c:Lcom/kamcord/a/a/d/KC_j;

    iget-object v1, p0, Lcom/kamcord/android/KC_B;->a:Lcom/kamcord/a/a/e/KC_c;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v2

    invoke-interface {v2}, Landroid/content/SharedPreferences;->edit()Landroid/content/SharedPreferences$Editor;

    move-result-object v2

    invoke-interface {v1, v0}, Lcom/kamcord/a/a/e/KC_c;->a(Lcom/kamcord/a/a/d/KC_j;)Ljava/util/HashMap;

    move-result-object v0

    if-eqz v0, :cond_1

    invoke-virtual {v0}, Ljava/util/HashMap;->entrySet()Ljava/util/Set;

    move-result-object v0

    invoke-interface {v0}, Ljava/util/Set;->iterator()Ljava/util/Iterator;

    move-result-object v3

    :cond_0
    :goto_1
    invoke-interface {v3}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_2

    invoke-interface {v3}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/util/Map$Entry;

    invoke-interface {v0}, Ljava/util/Map$Entry;->getKey()Ljava/lang/Object;

    move-result-object v1

    check-cast v1, Ljava/lang/String;

    const-string v4, "youtube_refresh_token"

    invoke-virtual {v1, v4}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v1

    if-nez v1, :cond_0

    invoke-interface {v0}, Ljava/util/Map$Entry;->getKey()Ljava/lang/Object;

    move-result-object v1

    check-cast v1, Ljava/lang/String;

    invoke-interface {v0}, Ljava/util/Map$Entry;->getValue()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/lang/String;

    invoke-interface {v2, v1, v0}, Landroid/content/SharedPreferences$Editor;->putString(Ljava/lang/String;Ljava/lang/String;)Landroid/content/SharedPreferences$Editor;

    goto :goto_1

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Ljava/lang/Exception;->printStackTrace()V

    goto :goto_0

    :cond_1
    invoke-interface {v1}, Lcom/kamcord/a/a/e/KC_c;->a()V

    :cond_2
    invoke-interface {v2}, Landroid/content/SharedPreferences$Editor;->apply()V

    :cond_3
    const/4 v0, 0x0

    return-object v0
.end method


# virtual methods
.method protected final synthetic doInBackground([Ljava/lang/Object;)Ljava/lang/Object;
    .locals 1

    invoke-direct {p0}, Lcom/kamcord/android/KC_B;->a()Ljava/lang/Void;

    move-result-object v0

    return-object v0
.end method
