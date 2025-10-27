.class final Lcom/kamcord/android/KC_w$KC_b;
.super Landroid/os/Handler;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/KC_w;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x8
    name = "KC_b"
.end annotation


# instance fields
.field private final a:Ljava/lang/ref/WeakReference;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/lang/ref/WeakReference",
            "<",
            "Lcom/kamcord/android/KC_w;",
            ">;"
        }
    .end annotation
.end field


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_w;)V
    .locals 1

    invoke-direct {p0}, Landroid/os/Handler;-><init>()V

    new-instance v0, Ljava/lang/ref/WeakReference;

    invoke-direct {v0, p1}, Ljava/lang/ref/WeakReference;-><init>(Ljava/lang/Object;)V

    iput-object v0, p0, Lcom/kamcord/android/KC_w$KC_b;->a:Ljava/lang/ref/WeakReference;

    return-void
.end method


# virtual methods
.method public final handleMessage(Landroid/os/Message;)V
    .locals 4

    const/4 v3, 0x2

    iget-object v0, p0, Lcom/kamcord/android/KC_w$KC_b;->a:Ljava/lang/ref/WeakReference;

    invoke-virtual {v0}, Ljava/lang/ref/WeakReference;->get()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_w;

    if-eqz v0, :cond_0

    invoke-static {v0}, Lcom/kamcord/android/KC_w;->c(Lcom/kamcord/android/KC_w;)Lcom/kamcord/android/KC_E;

    move-result-object v1

    if-nez v1, :cond_1

    :cond_0
    :goto_0
    return-void

    :cond_1
    iget v1, p1, Landroid/os/Message;->what:I

    packed-switch v1, :pswitch_data_0

    goto :goto_0

    :pswitch_0
    invoke-static {v0}, Lcom/kamcord/android/KC_w;->c(Lcom/kamcord/android/KC_w;)Lcom/kamcord/android/KC_E;

    move-result-object v1

    invoke-interface {v1}, Lcom/kamcord/android/KC_E;->isPlaying()Z

    move-result v1

    if-eqz v1, :cond_0

    invoke-virtual {v0}, Lcom/kamcord/android/KC_w;->d()V

    goto :goto_0

    :pswitch_1
    invoke-virtual {v0}, Lcom/kamcord/android/KC_w;->e()I

    move-result v1

    invoke-static {v0}, Lcom/kamcord/android/KC_w;->j(Lcom/kamcord/android/KC_w;)Z

    move-result v2

    if-nez v2, :cond_0

    invoke-static {v0}, Lcom/kamcord/android/KC_w;->k(Lcom/kamcord/android/KC_w;)Z

    move-result v2

    if-eqz v2, :cond_0

    invoke-static {v0}, Lcom/kamcord/android/KC_w;->c(Lcom/kamcord/android/KC_w;)Lcom/kamcord/android/KC_E;

    move-result-object v0

    invoke-interface {v0}, Lcom/kamcord/android/KC_E;->isPlaying()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-virtual {p0, v3}, Lcom/kamcord/android/KC_w$KC_b;->removeMessages(I)V

    invoke-virtual {p0, v3}, Lcom/kamcord/android/KC_w$KC_b;->obtainMessage(I)Landroid/os/Message;

    move-result-object v0

    rem-int/lit8 v1, v1, 0x64

    rsub-int/lit8 v1, v1, 0x64

    int-to-long v2, v1

    invoke-virtual {p0, v0, v2, v3}, Lcom/kamcord/android/KC_w$KC_b;->sendMessageDelayed(Landroid/os/Message;J)Z

    goto :goto_0

    :pswitch_data_0
    .packed-switch 0x1
        :pswitch_0
        :pswitch_1
    .end packed-switch
.end method
