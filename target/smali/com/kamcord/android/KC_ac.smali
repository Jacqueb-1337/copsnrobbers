.class public abstract Lcom/kamcord/android/KC_ac;
.super Landroid/os/Handler;


# instance fields
.field protected a:Z

.field private b:Landroid/os/Looper;


# direct methods
.method protected constructor <init>(Landroid/os/Looper;)V
    .locals 1

    invoke-direct {p0, p1}, Landroid/os/Handler;-><init>(Landroid/os/Looper;)V

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/KC_ac;->a:Z

    iput-object p1, p0, Lcom/kamcord/android/KC_ac;->b:Landroid/os/Looper;

    return-void
.end method


# virtual methods
.method public abstract a(Landroid/os/Message;)V
.end method

.method public abstract b(Landroid/os/Message;)V
.end method

.method public final handleMessage(Landroid/os/Message;)V
    .locals 2

    :try_start_0
    iget v0, p1, Landroid/os/Message;->what:I

    const v1, 0x7fffffff

    if-ne v0, v1, :cond_2

    iget-object v0, p0, Lcom/kamcord/android/KC_ac;->b:Landroid/os/Looper;

    invoke-virtual {v0}, Landroid/os/Looper;->quit()V

    :cond_0
    :goto_0
    iget-boolean v0, p0, Lcom/kamcord/android/KC_ac;->a:Z

    if-nez v0, :cond_1

    invoke-virtual {p0, p1}, Lcom/kamcord/android/KC_ac;->a(Landroid/os/Message;)V

    :cond_1
    :goto_1
    return-void

    :cond_2
    iget-boolean v0, p0, Lcom/kamcord/android/KC_ac;->a:Z

    if-eqz v0, :cond_0

    invoke-virtual {p0, p1}, Lcom/kamcord/android/KC_ac;->b(Landroid/os/Message;)V
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    goto :goto_0

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V

    const-string v0, "Disabling all kamcord threads..."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/KC_ad;->b()V

    const-string v0, "Bailing out..."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/Kamcord;->bail()V

    goto :goto_1
.end method
