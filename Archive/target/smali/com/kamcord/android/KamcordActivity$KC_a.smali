.class public final Lcom/kamcord/android/KamcordActivity$KC_a;
.super La/a/a/a/KC_k;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/KamcordActivity;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x9
    name = "KC_a"
.end annotation


# instance fields
.field private a:Lcom/kamcord/android/KC_q;

.field private b:I


# direct methods
.method public constructor <init>(La/a/a/a/KC_h;Lcom/kamcord/android/KC_q;I)V
    .locals 1

    invoke-direct {p0, p1}, La/a/a/a/KC_k;-><init>(La/a/a/a/KC_h;)V

    const/4 v0, 0x0

    iput v0, p0, Lcom/kamcord/android/KamcordActivity$KC_a;->b:I

    iput-object p2, p0, Lcom/kamcord/android/KamcordActivity$KC_a;->a:Lcom/kamcord/android/KC_q;

    iput p3, p0, Lcom/kamcord/android/KamcordActivity$KC_a;->b:I

    return-void
.end method


# virtual methods
.method public final a()I
    .locals 2

    iget v0, p0, Lcom/kamcord/android/KamcordActivity$KC_a;->b:I

    if-nez v0, :cond_0

    const/4 v0, 0x3

    :goto_0
    return v0

    :cond_0
    iget v0, p0, Lcom/kamcord/android/KamcordActivity$KC_a;->b:I

    const/4 v1, 0x1

    if-ne v0, v1, :cond_1

    const/4 v0, 0x2

    goto :goto_0

    :cond_1
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public final a(I)La/a/a/a/KC_e;
    .locals 4

    const/4 v1, 0x0

    new-instance v2, Landroid/os/Bundle;

    invoke-direct {v2}, Landroid/os/Bundle;-><init>()V

    sget-object v3, Lcom/kamcord/android/KamcordActivity$2;->a:[I

    iget-object v0, p0, Lcom/kamcord/android/KamcordActivity$KC_a;->a:Lcom/kamcord/android/KC_q;

    invoke-virtual {v0, p1}, Lcom/kamcord/android/KC_q;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/c/KC_a;

    invoke-virtual {v0}, La/a/a/c/KC_a;->b()Lcom/kamcord/android/KC_p;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/KC_p;->ordinal()I

    move-result v0

    aget v0, v3, v0

    packed-switch v0, :pswitch_data_0

    move-object v0, v1

    :goto_0
    return-object v0

    :pswitch_0
    new-instance v0, Lcom/kamcord/android/KC_N;

    invoke-direct {v0}, Lcom/kamcord/android/KC_N;-><init>()V

    goto :goto_0

    :pswitch_1
    new-instance v0, Lcom/kamcord/android/KC_J;

    invoke-direct {v0}, Lcom/kamcord/android/KC_J;-><init>()V

    const-string v1, "type"

    const/4 v3, 0x0

    invoke-virtual {v2, v1, v3}, Landroid/os/Bundle;->putInt(Ljava/lang/String;I)V

    invoke-virtual {v0, v2}, La/a/a/a/KC_e;->f(Landroid/os/Bundle;)V

    goto :goto_0

    :pswitch_2
    new-instance v0, Lcom/kamcord/android/KC_J;

    invoke-direct {v0}, Lcom/kamcord/android/KC_J;-><init>()V

    const-string v1, "type"

    const/4 v3, 0x1

    invoke-virtual {v2, v1, v3}, Landroid/os/Bundle;->putInt(Ljava/lang/String;I)V

    invoke-virtual {v0, v2}, La/a/a/a/KC_e;->f(Landroid/os/Bundle;)V

    goto :goto_0

    nop

    :pswitch_data_0
    .packed-switch 0x1
        :pswitch_0
        :pswitch_1
        :pswitch_2
    .end packed-switch
.end method
