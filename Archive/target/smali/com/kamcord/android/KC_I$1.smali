.class final Lcom/kamcord/android/KC_I$1;
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

    iput-object p1, p0, Lcom/kamcord/android/KC_I$1;->a:Lcom/kamcord/android/KC_I;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onClick(Landroid/view/View;)V
    .locals 2

    new-instance v0, Lcom/kamcord/android/KC_i;

    invoke-direct {v0}, Lcom/kamcord/android/KC_i;-><init>()V

    iget-object v1, p0, Lcom/kamcord/android/KC_I$1;->a:Lcom/kamcord/android/KC_I;

    invoke-static {v1}, Lcom/kamcord/android/KC_I;->a(Lcom/kamcord/android/KC_I;)Lcom/kamcord/android/KamcordActivity;

    move-result-object v1

    invoke-virtual {v1}, Lcom/kamcord/android/KamcordActivity;->getTabFragmentManager()Lcom/kamcord/android/KC_T;

    move-result-object v1

    invoke-virtual {v1, v0}, Lcom/kamcord/android/KC_T;->a(La/a/a/a/KC_e;)V

    return-void
.end method
