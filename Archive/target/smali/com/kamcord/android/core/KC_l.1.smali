.class final Lcom/kamcord/android/core/KC_l;
.super Lcom/kamcord/android/KC_ad;


# instance fields
.field private b:Lcom/kamcord/android/core/KC_j;


# direct methods
.method constructor <init>(Lcom/kamcord/android/core/KC_j;)V
    .locals 1

    const-string v0, "input-to-codec"

    invoke-direct {p0, v0}, Lcom/kamcord/android/KC_ad;-><init>(Ljava/lang/String;)V

    iput-object p1, p0, Lcom/kamcord/android/core/KC_l;->b:Lcom/kamcord/android/core/KC_j;

    return-void
.end method


# virtual methods
.method protected final a()Lcom/kamcord/android/KC_ac;
    .locals 3

    new-instance v0, Lcom/kamcord/android/core/KC_k;

    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_l;->getLooper()Landroid/os/Looper;

    move-result-object v1

    iget-object v2, p0, Lcom/kamcord/android/core/KC_l;->b:Lcom/kamcord/android/core/KC_j;

    invoke-direct {v0, v1, v2}, Lcom/kamcord/android/core/KC_k;-><init>(Landroid/os/Looper;Lcom/kamcord/android/core/KC_j;)V

    return-object v0
.end method

.method final a(Lcom/kamcord/android/core/KC_F;)V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/core/KC_l;->a:Lcom/kamcord/android/KC_ac;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_l;->a:Lcom/kamcord/android/KC_ac;

    const/4 v2, 0x3

    invoke-static {v1, v2, p1}, Landroid/os/Message;->obtain(Landroid/os/Handler;ILjava/lang/Object;)Landroid/os/Message;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_ac;->sendMessage(Landroid/os/Message;)Z

    return-void
.end method

.method final a(Lcom/kamcord/android/core/KC_g;)V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/core/KC_l;->a:Lcom/kamcord/android/KC_ac;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_l;->a:Lcom/kamcord/android/KC_ac;

    const/4 v2, 0x2

    invoke-static {v1, v2, p1}, Landroid/os/Message;->obtain(Landroid/os/Handler;ILjava/lang/Object;)Landroid/os/Message;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_ac;->sendMessage(Landroid/os/Message;)Z

    return-void
.end method

.method final e()V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/core/KC_l;->a:Lcom/kamcord/android/KC_ac;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_l;->a:Lcom/kamcord/android/KC_ac;

    const/4 v2, 0x1

    invoke-static {v1, v2}, Landroid/os/Message;->obtain(Landroid/os/Handler;I)Landroid/os/Message;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_ac;->sendMessage(Landroid/os/Message;)Z

    return-void
.end method

.method final f()V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/core/KC_l;->a:Lcom/kamcord/android/KC_ac;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_l;->a:Lcom/kamcord/android/KC_ac;

    const/4 v2, 0x4

    invoke-static {v1, v2}, Landroid/os/Message;->obtain(Landroid/os/Handler;I)Landroid/os/Message;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_ac;->sendMessage(Landroid/os/Message;)Z

    return-void
.end method
