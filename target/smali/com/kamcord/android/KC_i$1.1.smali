.class final Lcom/kamcord/android/KC_i$1;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/view/View$OnClickListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_i;->a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/KC_i;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_i;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_i$1;->a:Lcom/kamcord/android/KC_i;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onClick(Landroid/view/View;)V
    .locals 4

    new-instance v0, Lcom/kamcord/android/KC_f;

    invoke-direct {v0}, Lcom/kamcord/android/KC_f;-><init>()V

    new-instance v1, Landroid/os/Bundle;

    invoke-direct {v1}, Landroid/os/Bundle;-><init>()V

    const-string v2, "credType"

    sget-object v3, Lcom/kamcord/android/KC_f$KC_a;->a:Lcom/kamcord/android/KC_f$KC_a;

    invoke-virtual {v1, v2, v3}, Landroid/os/Bundle;->putSerializable(Ljava/lang/String;Ljava/io/Serializable;)V

    invoke-virtual {v0, v1}, La/a/a/a/KC_e;->f(Landroid/os/Bundle;)V

    iget-object v1, p0, Lcom/kamcord/android/KC_i$1;->a:Lcom/kamcord/android/KC_i;

    iget-object v1, v1, Lcom/kamcord/android/KC_i;->M:Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v1}, Lcom/kamcord/android/KamcordActivity;->getTabFragmentManager()Lcom/kamcord/android/KC_T;

    move-result-object v1

    invoke-virtual {v1, v0}, Lcom/kamcord/android/KC_T;->a(La/a/a/a/KC_e;)V

    return-void
.end method
