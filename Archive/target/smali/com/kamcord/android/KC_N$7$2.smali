.class final Lcom/kamcord/android/KC_N$7$2;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/content/DialogInterface$OnClickListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_N$7;->onCheckedChanged(Landroid/widget/CompoundButton;Z)V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/KC_N$7;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_N$7;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_N$7$2;->a:Lcom/kamcord/android/KC_N$7;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onClick(Landroid/content/DialogInterface;I)V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_N$7$2;->a:Lcom/kamcord/android/KC_N$7;

    iget-object v0, v0, Lcom/kamcord/android/KC_N$7;->a:Lcom/kamcord/android/KC_N;

    iget-object v0, v0, Lcom/kamcord/android/KC_N;->R:Landroid/widget/ToggleButton;

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Landroid/widget/ToggleButton;->setChecked(Z)V

    return-void
.end method
