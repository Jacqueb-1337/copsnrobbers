.class final Lcom/kamcord/android/KC_q$7;
.super Ljava/lang/Object;

# interfaces
.implements Ljava/lang/Runnable;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_q;->a_(Ljava/lang/String;Z)V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Z

.field private synthetic b:Lcom/kamcord/android/KC_q;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_q;Z)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_q$7;->b:Lcom/kamcord/android/KC_q;

    iput-boolean p2, p0, Lcom/kamcord/android/KC_q$7;->a:Z

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final run()V
    .locals 6

    :try_start_0
    iget-boolean v0, p0, Lcom/kamcord/android/KC_q$7;->a:Z

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_q$7;->b:Lcom/kamcord/android/KC_q;

    invoke-static {v0}, Lcom/kamcord/android/KC_q;->c(Lcom/kamcord/android/KC_q;)Ljava/lang/ref/WeakReference;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/ref/WeakReference;->get()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Landroid/content/Context;

    const-string v1, "kamcordUploadFinished"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    const/4 v2, 0x0

    invoke-static {v0, v1, v2}, Landroid/widget/Toast;->makeText(Landroid/content/Context;Ljava/lang/CharSequence;I)Landroid/widget/Toast;

    move-result-object v0

    :goto_0
    invoke-virtual {v0}, Landroid/widget/Toast;->show()V

    new-instance v1, Landroid/os/Handler;

    invoke-direct {v1}, Landroid/os/Handler;-><init>()V

    new-instance v2, Lcom/kamcord/android/KC_q$7$1;

    invoke-direct {v2, p0, v0}, Lcom/kamcord/android/KC_q$7$1;-><init>(Lcom/kamcord/android/KC_q$7;Landroid/widget/Toast;)V

    const-wide/16 v4, 0x3e8

    invoke-virtual {v1, v2, v4, v5}, Landroid/os/Handler;->postDelayed(Ljava/lang/Runnable;J)Z

    :goto_1
    return-void

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_q$7;->b:Lcom/kamcord/android/KC_q;

    invoke-static {v0}, Lcom/kamcord/android/KC_q;->c(Lcom/kamcord/android/KC_q;)Ljava/lang/ref/WeakReference;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/ref/WeakReference;->get()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Landroid/content/Context;

    const-string v1, "kamcordUploadFailed"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    const/4 v2, 0x0

    invoke-static {v0, v1, v2}, Landroid/widget/Toast;->makeText(Landroid/content/Context;Ljava/lang/CharSequence;I)Landroid/widget/Toast;
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    move-result-object v0

    goto :goto_0

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V

    goto :goto_1
.end method
