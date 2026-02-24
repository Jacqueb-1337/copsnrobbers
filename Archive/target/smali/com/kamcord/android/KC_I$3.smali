.class final Lcom/kamcord/android/KC_I$3;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/view/View$OnClickListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_I;->a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/KC_I;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_I;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_I$3;->a:Lcom/kamcord/android/KC_I;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onClick(Landroid/view/View;)V
    .locals 4

    invoke-static {}, La/a/a/c/KC_a;->a()Z

    move-result v0

    if-nez v0, :cond_0

    new-instance v0, Lcom/kamcord/android/KC_x;

    invoke-direct {v0}, Lcom/kamcord/android/KC_x;-><init>()V

    iget-object v1, p0, Lcom/kamcord/android/KC_I$3;->a:Lcom/kamcord/android/KC_I;

    invoke-static {v1}, Lcom/kamcord/android/KC_I;->a(Lcom/kamcord/android/KC_I;)Lcom/kamcord/android/KamcordActivity;

    move-result-object v1

    invoke-virtual {v1}, Lcom/kamcord/android/KamcordActivity;->getFragmentManager()Landroid/app/FragmentManager;

    move-result-object v1

    const/4 v2, 0x0

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/android/KC_x;->show(Landroid/app/FragmentManager;Ljava/lang/String;)V

    :goto_0
    return-void

    :cond_0
    new-instance v0, Lcom/kamcord/android/KC_ab;

    invoke-direct {v0}, Lcom/kamcord/android/KC_ab;-><init>()V

    new-instance v1, Landroid/os/Bundle;

    invoke-direct {v1}, Landroid/os/Bundle;-><init>()V

    const-string v2, "url"

    const-string v3, "https://www.kamcord.com/privacy/"

    invoke-virtual {v1, v2, v3}, Landroid/os/Bundle;->putString(Ljava/lang/String;Ljava/lang/String;)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_ab;->f(Landroid/os/Bundle;)V

    iget-object v1, p0, Lcom/kamcord/android/KC_I$3;->a:Lcom/kamcord/android/KC_I;

    invoke-static {v1}, Lcom/kamcord/android/KC_I;->a(Lcom/kamcord/android/KC_I;)Lcom/kamcord/android/KamcordActivity;

    move-result-object v1

    invoke-virtual {v1}, Lcom/kamcord/android/KamcordActivity;->getTabFragmentManager()Lcom/kamcord/android/KC_T;

    move-result-object v1

    invoke-virtual {v1, v0}, Lcom/kamcord/android/KC_T;->a(La/a/a/a/KC_e;)V

    goto :goto_0
.end method
