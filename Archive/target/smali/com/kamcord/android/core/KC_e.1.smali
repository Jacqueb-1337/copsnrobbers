.class final Lcom/kamcord/android/core/KC_e;
.super Lcom/kamcord/android/KC_ad;


# instance fields
.field private b:Lcom/kamcord/android/core/KC_T;

.field private c:J


# direct methods
.method constructor <init>(Lcom/kamcord/android/core/KC_T;)V
    .locals 2

    const-string v0, "codec-to-muxer"

    invoke-direct {p0, v0}, Lcom/kamcord/android/KC_ad;-><init>(Ljava/lang/String;)V

    const-wide/16 v0, 0x0

    iput-wide v0, p0, Lcom/kamcord/android/core/KC_e;->c:J

    iput-object p1, p0, Lcom/kamcord/android/core/KC_e;->b:Lcom/kamcord/android/core/KC_T;

    return-void
.end method


# virtual methods
.method protected final a()Lcom/kamcord/android/KC_ac;
    .locals 3

    new-instance v0, Lcom/kamcord/android/core/KC_d;

    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_e;->getLooper()Landroid/os/Looper;

    move-result-object v1

    iget-object v2, p0, Lcom/kamcord/android/core/KC_e;->b:Lcom/kamcord/android/core/KC_T;

    invoke-direct {v0, v1, v2}, Lcom/kamcord/android/core/KC_d;-><init>(Landroid/os/Looper;Lcom/kamcord/android/core/KC_T;)V

    return-object v0
.end method

.method final a(D)V
    .locals 5

    iget-object v0, p0, Lcom/kamcord/android/core/KC_e;->a:Lcom/kamcord/android/KC_ac;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_e;->a:Lcom/kamcord/android/KC_ac;

    const/4 v2, 0x3

    invoke-static {p1, p2}, Ljava/lang/Double;->valueOf(D)Ljava/lang/Double;

    move-result-object v3

    invoke-static {v1, v2, v3}, Landroid/os/Message;->obtain(Landroid/os/Handler;ILjava/lang/Object;)Landroid/os/Message;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_ac;->sendMessage(Landroid/os/Message;)Z

    return-void
.end method

.method final e()V
    .locals 6

    invoke-static {}, Ljava/lang/System;->nanoTime()J

    move-result-wide v0

    iget-wide v2, p0, Lcom/kamcord/android/core/KC_e;->c:J

    sub-long v2, v0, v2

    const-wide v4, 0x2540be400L

    cmp-long v2, v2, v4

    if-lez v2, :cond_0

    const/4 v2, 0x2

    invoke-static {v2}, Lcom/kamcord/android/Kamcord;->setCurrentThreadAffinity(I)V

    iput-wide v0, p0, Lcom/kamcord/android/core/KC_e;->c:J

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_e;->a:Lcom/kamcord/android/KC_ac;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_e;->a:Lcom/kamcord/android/KC_ac;

    const/4 v2, 0x1

    invoke-static {v1, v2}, Landroid/os/Message;->obtain(Landroid/os/Handler;I)Landroid/os/Message;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_ac;->sendMessage(Landroid/os/Message;)Z

    return-void
.end method

.method final f()V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/core/KC_e;->a:Lcom/kamcord/android/KC_ac;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_e;->a:Lcom/kamcord/android/KC_ac;

    const/4 v2, 0x2

    invoke-static {v1, v2}, Landroid/os/Message;->obtain(Landroid/os/Handler;I)Landroid/os/Message;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_ac;->sendMessage(Landroid/os/Message;)Z

    return-void
.end method
