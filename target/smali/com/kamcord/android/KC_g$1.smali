.class final Lcom/kamcord/android/KC_g$1;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/content/DialogInterface$OnClickListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_g;->onCreateDialog(Landroid/os/Bundle;)Landroid/app/Dialog;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/KC_g;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_g;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_g$1;->a:Lcom/kamcord/android/KC_g;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onClick(Landroid/content/DialogInterface;I)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_g$1;->a:Lcom/kamcord/android/KC_g;

    invoke-static {v0}, Lcom/kamcord/android/KC_g;->a(Lcom/kamcord/android/KC_g;)Lcom/kamcord/android/KC_f;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/KC_f;->B()V

    return-void
.end method
