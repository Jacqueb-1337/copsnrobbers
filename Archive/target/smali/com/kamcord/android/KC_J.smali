.class public final Lcom/kamcord/android/KC_J;
.super La/a/a/a/KC_e;


# annotations
.annotation build Landroid/annotation/SuppressLint;
    value = {
        "ValidFragment"
    }
.end annotation


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, La/a/a/a/KC_e;-><init>()V

    return-void
.end method


# virtual methods
.method public final a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;
    .locals 5

    const/4 v2, -0x1

    new-instance v1, Landroid/widget/FrameLayout;

    invoke-virtual {p0}, Lcom/kamcord/android/KC_J;->h()La/a/a/a/KC_f;

    move-result-object v0

    invoke-direct {v1, v0}, Landroid/widget/FrameLayout;-><init>(Landroid/content/Context;)V

    new-instance v0, Landroid/widget/FrameLayout$LayoutParams;

    invoke-direct {v0, v2, v2}, Landroid/widget/FrameLayout$LayoutParams;-><init>(II)V

    invoke-virtual {v1, v0}, Landroid/widget/FrameLayout;->setLayoutParams(Landroid/view/ViewGroup$LayoutParams;)V

    invoke-virtual {p0}, Lcom/kamcord/android/KC_J;->g()Landroid/os/Bundle;

    move-result-object v0

    :try_start_0
    const-string v2, "type"

    const/4 v3, -0x1

    invoke-virtual {v0, v2, v3}, Landroid/os/Bundle;->getInt(Ljava/lang/String;I)I
    :try_end_0
    .catch Ljava/lang/NullPointerException; {:try_start_0 .. :try_end_0} :catch_0

    move-result v0

    packed-switch v0, :pswitch_data_0

    const-string v0, "Error: invalid initial fragment type."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    move-object v0, v1

    :goto_0
    return-object v0

    :catch_0
    move-exception v0

    const-string v2, "No arguments given to ReplaceableFragment!"

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/NullPointerException;->printStackTrace()V

    move-object v0, v1

    goto :goto_0

    :pswitch_0
    new-instance v2, Lcom/kamcord/android/KC_aa;

    invoke-direct {v2}, Lcom/kamcord/android/KC_aa;-><init>()V

    invoke-virtual {p0}, Lcom/kamcord/android/KC_J;->h()La/a/a/a/KC_f;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordActivity;

    sget-object v3, Lcom/kamcord/android/KC_p;->b:Lcom/kamcord/android/KC_p;

    invoke-virtual {v0, v3}, Lcom/kamcord/android/KamcordActivity;->indexOfTabType(Lcom/kamcord/android/KC_p;)I

    move-result v0

    const v3, 0x7b78540

    invoke-virtual {v1, v3}, Landroid/widget/FrameLayout;->setId(I)V

    move-object v3, v2

    move v2, v0

    :goto_1
    if-gez v2, :cond_0

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v3, "Invalid tab index: "

    invoke-direct {v0, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, v2}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    move-object v0, v1

    goto :goto_0

    :pswitch_1
    new-instance v2, Lcom/kamcord/android/KC_F;

    invoke-direct {v2}, Lcom/kamcord/android/KC_F;-><init>()V

    invoke-virtual {p0}, Lcom/kamcord/android/KC_J;->h()La/a/a/a/KC_f;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordActivity;

    sget-object v3, Lcom/kamcord/android/KC_p;->c:Lcom/kamcord/android/KC_p;

    invoke-virtual {v0, v3}, Lcom/kamcord/android/KamcordActivity;->indexOfTabType(Lcom/kamcord/android/KC_p;)I

    move-result v0

    const v3, 0x6403276

    invoke-virtual {v1, v3}, Landroid/widget/FrameLayout;->setId(I)V

    move-object v3, v2

    move v2, v0

    goto :goto_1

    :cond_0
    invoke-virtual {p0}, Lcom/kamcord/android/KC_J;->h()La/a/a/a/KC_f;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v0}, Lcom/kamcord/android/KamcordActivity;->getTabFragmentManager()Lcom/kamcord/android/KC_T;

    move-result-object v0

    invoke-virtual {v1}, Landroid/widget/FrameLayout;->getId()I

    move-result v4

    invoke-virtual {v0, v4, v3, v2}, Lcom/kamcord/android/KC_T;->a(ILa/a/a/a/KC_e;I)V

    move-object v0, v1

    goto :goto_0

    :pswitch_data_0
    .packed-switch 0x0
        :pswitch_0
        :pswitch_1
    .end packed-switch
.end method
