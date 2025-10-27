.class public final Lcom/kamcord/android/KC_C;
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
.field private a:Lcom/kamcord/a/a/d/KC_c;

.field private b:Lcom/kamcord/a/a/e/KC_c;

.field private c:Lcom/kamcord/a/a/d/KC_h;

.field private d:Ljava/lang/String;


# direct methods
.method public constructor <init>(Lcom/kamcord/a/a/d/KC_c;Lcom/kamcord/a/a/e/KC_c;Ljava/lang/String;)V
    .locals 0

    invoke-direct {p0}, Landroid/os/AsyncTask;-><init>()V

    iput-object p1, p0, Lcom/kamcord/android/KC_C;->a:Lcom/kamcord/a/a/d/KC_c;

    iput-object p2, p0, Lcom/kamcord/android/KC_C;->b:Lcom/kamcord/a/a/e/KC_c;

    iput-object p3, p0, Lcom/kamcord/android/KC_C;->d:Ljava/lang/String;

    return-void
.end method

.method private varargs a()Ljava/lang/Void;
    .locals 3

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/KC_C;->a:Lcom/kamcord/a/a/d/KC_c;

    invoke-virtual {v0}, Lcom/kamcord/a/a/d/KC_c;->b()Lcom/kamcord/a/a/d/KC_h;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_C;->c:Lcom/kamcord/a/a/d/KC_h;
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    :goto_0
    iget-object v0, p0, Lcom/kamcord/android/KC_C;->c:Lcom/kamcord/a/a/d/KC_h;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_C;->c:Lcom/kamcord/a/a/d/KC_h;

    invoke-virtual {v0}, Lcom/kamcord/a/a/d/KC_h;->a()Z

    move-result v0

    if-nez v0, :cond_1

    :cond_0
    invoke-static {}, Lcom/kamcord/android/Kamcord;->getAuthCenter()Lcom/kamcord/android/KC_e;

    move-result-object v0

    iget-object v1, p0, Lcom/kamcord/android/KC_C;->b:Lcom/kamcord/a/a/e/KC_c;

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_e;->a(Lcom/kamcord/a/a/e/KC_c;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_C;->d:Ljava/lang/String;

    iget-object v1, p0, Lcom/kamcord/android/KC_C;->b:Lcom/kamcord/a/a/e/KC_c;

    invoke-interface {v1}, Lcom/kamcord/a/a/e/KC_c;->b()Ljava/lang/String;

    move-result-object v1

    const/4 v2, 0x0

    invoke-static {v0, v1, v2}, Lcom/kamcord/android/Kamcord;->notifyVideoSharedTo(Ljava/lang/String;Ljava/lang/String;Z)V

    :goto_1
    const/4 v0, 0x0

    return-object v0

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Ljava/lang/Exception;->printStackTrace()V

    goto :goto_0

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/KC_C;->d:Ljava/lang/String;

    iget-object v1, p0, Lcom/kamcord/android/KC_C;->b:Lcom/kamcord/a/a/e/KC_c;

    invoke-interface {v1}, Lcom/kamcord/a/a/e/KC_c;->b()Ljava/lang/String;

    move-result-object v1

    const/4 v2, 0x1

    invoke-static {v0, v1, v2}, Lcom/kamcord/android/Kamcord;->notifyVideoSharedTo(Ljava/lang/String;Ljava/lang/String;Z)V

    goto :goto_1
.end method


# virtual methods
.method protected final synthetic doInBackground([Ljava/lang/Object;)Ljava/lang/Object;
    .locals 1

    invoke-direct {p0}, Lcom/kamcord/android/KC_C;->a()Ljava/lang/Void;

    move-result-object v0

    return-object v0
.end method
