.class final Lcom/kamcord/android/KC_b;
.super Lcom/kamcord/android/KC_ac;


# instance fields
.field private b:Lcom/kamcord/android/KC_c;


# direct methods
.method public constructor <init>(Landroid/os/Looper;Lcom/kamcord/android/KC_c;)V
    .locals 0

    invoke-direct {p0, p1}, Lcom/kamcord/android/KC_ac;-><init>(Landroid/os/Looper;)V

    iput-object p2, p0, Lcom/kamcord/android/KC_b;->b:Lcom/kamcord/android/KC_c;

    return-void
.end method


# virtual methods
.method public final a(Landroid/os/Message;)V
    .locals 2

    iget v0, p1, Landroid/os/Message;->what:I

    packed-switch v0, :pswitch_data_0

    :goto_0
    return-void

    :pswitch_0
    iget-object v0, p0, Lcom/kamcord/android/KC_b;->b:Lcom/kamcord/android/KC_c;

    invoke-virtual {p1}, Landroid/os/Message;->getData()Landroid/os/Bundle;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_c;->a(Landroid/os/Bundle;)V

    goto :goto_0

    :pswitch_1
    iget-object v0, p0, Lcom/kamcord/android/KC_b;->b:Lcom/kamcord/android/KC_c;

    invoke-virtual {p1}, Landroid/os/Message;->getData()Landroid/os/Bundle;

    move-result-object v0

    const-string v1, "event"

    invoke-virtual {v0, v1}, Landroid/os/Bundle;->getInt(Ljava/lang/String;)I

    move-result v0

    invoke-static {v0}, Lcom/kamcord/android/KC_c;->a(I)V

    invoke-static {}, Lcom/kamcord/android/KC_m;->a()V

    goto :goto_0

    nop

    :pswitch_data_0
    .packed-switch 0x2
        :pswitch_0
        :pswitch_1
    .end packed-switch
.end method

.method public final b(Landroid/os/Message;)V
    .locals 0

    return-void
.end method
