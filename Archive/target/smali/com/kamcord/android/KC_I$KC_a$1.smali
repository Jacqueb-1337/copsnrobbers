.class final Lcom/kamcord/android/KC_I$KC_a$1;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/content/DialogInterface$OnClickListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_I$KC_a;->onCreateDialog(Landroid/os/Bundle;)Landroid/app/Dialog;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/KC_I$KC_a;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_I$KC_a;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_I$KC_a$1;->a:Lcom/kamcord/android/KC_I$KC_a;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onClick(Landroid/content/DialogInterface;I)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_I$KC_a$1;->a:Lcom/kamcord/android/KC_I$KC_a;

    iget-object v0, v0, Lcom/kamcord/android/KC_I$KC_a;->a:Lcom/kamcord/android/KC_I;

    invoke-static {v0}, Lcom/kamcord/android/KC_I;->b(Lcom/kamcord/android/KC_I;)Landroid/view/View;

    move-result-object v0

    invoke-virtual {v0}, Landroid/view/View;->getParent()Landroid/view/ViewParent;

    move-result-object v0

    check-cast v0, Landroid/view/ViewGroup;

    invoke-static {v0}, La/a/a/c/KC_a;->a(Landroid/view/ViewGroup;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_I$KC_a$1;->a:Lcom/kamcord/android/KC_I$KC_a;

    invoke-static {v0}, Lcom/kamcord/android/KC_I$KC_a;->a(Lcom/kamcord/android/KC_I$KC_a;)Lcom/kamcord/android/b/KC_e;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/b/KC_e;->a()V

    return-void
.end method
