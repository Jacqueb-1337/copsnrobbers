.class final Lcom/kamcord/android/b/KC_f$KC_a;
.super Landroid/os/AsyncTask;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/b/KC_f;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = "KC_a"
.end annotation

.annotation system Ldalvik/annotation/Signature;
    value = {
        "Landroid/os/AsyncTask",
        "<",
        "Lcom/kamcord/a/a/e/KC_c;",
        "Ljava/lang/Void;",
        "Lcom/kamcord/a/a/d/KC_j;",
        ">;"
    }
.end annotation


# instance fields
.field private a:Lcom/kamcord/a/a/e/KC_c;


# direct methods
.method private constructor <init>(Lcom/kamcord/android/b/KC_f;)V
    .locals 0

    invoke-direct {p0}, Landroid/os/AsyncTask;-><init>()V

    return-void
.end method

.method synthetic constructor <init>(Lcom/kamcord/android/b/KC_f;B)V
    .locals 0

    invoke-direct {p0, p1}, Lcom/kamcord/android/b/KC_f$KC_a;-><init>(Lcom/kamcord/android/b/KC_f;)V

    return-void
.end method


# virtual methods
.method protected final synthetic doInBackground([Ljava/lang/Object;)Ljava/lang/Object;
    .locals 1

    check-cast p1, [Lcom/kamcord/a/a/e/KC_c;

    const/4 v0, 0x0

    aget-object v0, p1, v0

    iput-object v0, p0, Lcom/kamcord/android/b/KC_f$KC_a;->a:Lcom/kamcord/a/a/e/KC_c;

    iget-object v0, p0, Lcom/kamcord/android/b/KC_f$KC_a;->a:Lcom/kamcord/a/a/e/KC_c;

    invoke-interface {v0}, Lcom/kamcord/a/a/e/KC_c;->d()Lcom/kamcord/a/a/d/KC_j;

    move-result-object v0

    return-object v0
.end method

.method protected final synthetic onPostExecute(Ljava/lang/Object;)V
    .locals 2

    check-cast p1, Lcom/kamcord/a/a/d/KC_j;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getAuthCenter()Lcom/kamcord/android/KC_e;

    move-result-object v0

    iget-object v1, p0, Lcom/kamcord/android/b/KC_f$KC_a;->a:Lcom/kamcord/a/a/e/KC_c;

    invoke-virtual {v0, p1, v1}, Lcom/kamcord/android/KC_e;->a(Lcom/kamcord/a/a/d/KC_j;Lcom/kamcord/a/a/e/KC_c;)V

    return-void
.end method
