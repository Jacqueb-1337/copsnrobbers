.class final Lcom/kamcord/android/KC_A;
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

.field private d:Lcom/kamcord/a/a/d/KC_j;


# direct methods
.method constructor <init>(Lcom/kamcord/a/a/e/KC_c;Lcom/kamcord/a/a/d/KC_l;Lcom/kamcord/a/a/d/KC_j;)V
    .locals 0

    invoke-direct {p0}, Landroid/os/AsyncTask;-><init>()V

    iput-object p1, p0, Lcom/kamcord/android/KC_A;->a:Lcom/kamcord/a/a/e/KC_c;

    iput-object p2, p0, Lcom/kamcord/android/KC_A;->b:Lcom/kamcord/a/a/d/KC_l;

    iput-object p3, p0, Lcom/kamcord/android/KC_A;->d:Lcom/kamcord/a/a/d/KC_j;

    return-void
.end method

.method private varargs a()Ljava/lang/Void;
    .locals 3

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/KC_A;->a:Lcom/kamcord/a/a/e/KC_c;

    iget-object v1, p0, Lcom/kamcord/android/KC_A;->d:Lcom/kamcord/a/a/d/KC_j;

    iget-object v2, p0, Lcom/kamcord/android/KC_A;->b:Lcom/kamcord/a/a/d/KC_l;

    invoke-interface {v0, v1, v2}, Lcom/kamcord/a/a/e/KC_c;->a(Lcom/kamcord/a/a/d/KC_j;Lcom/kamcord/a/a/d/KC_l;)Lcom/kamcord/a/a/d/KC_j;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_A;->c:Lcom/kamcord/a/a/d/KC_j;
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    :goto_0
    iget-object v0, p0, Lcom/kamcord/android/KC_A;->c:Lcom/kamcord/a/a/d/KC_j;

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getAuthCenter()Lcom/kamcord/android/KC_e;

    move-result-object v0

    iget-object v1, p0, Lcom/kamcord/android/KC_A;->c:Lcom/kamcord/a/a/d/KC_j;

    iget-object v2, p0, Lcom/kamcord/android/KC_A;->a:Lcom/kamcord/a/a/e/KC_c;

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/android/KC_e;->b(Lcom/kamcord/a/a/d/KC_j;Lcom/kamcord/a/a/e/KC_c;)V

    :cond_0
    const/4 v0, 0x0

    return-object v0

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Ljava/lang/Exception;->printStackTrace()V

    goto :goto_0
.end method


# virtual methods
.method protected final synthetic doInBackground([Ljava/lang/Object;)Ljava/lang/Object;
    .locals 1

    invoke-direct {p0}, Lcom/kamcord/android/KC_A;->a()Ljava/lang/Void;

    move-result-object v0

    return-object v0
.end method
