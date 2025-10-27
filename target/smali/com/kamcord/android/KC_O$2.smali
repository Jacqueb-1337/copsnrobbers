.class final Lcom/kamcord/android/KC_O$2;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/content/DialogInterface$OnClickListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_O;->b()Landroid/app/Dialog;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/KC_O;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_O;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_O$2;->a:Lcom/kamcord/android/KC_O;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onClick(Landroid/content/DialogInterface;I)V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_O$2;->a:Lcom/kamcord/android/KC_O;

    iget-object v0, v0, Lcom/kamcord/android/KC_O;->M:Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v0}, Lcom/kamcord/android/KamcordActivity;->getTabFragmentManager()Lcom/kamcord/android/KC_T;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/KC_P;

    invoke-direct {v1}, Lcom/kamcord/android/KC_P;-><init>()V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_T;->a(La/a/a/a/KC_e;)V

    return-void
.end method
