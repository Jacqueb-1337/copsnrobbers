.class final Lcom/kamcord/android/KC_h$7;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/content/DialogInterface$OnDismissListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_h;->a(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/KC_h;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_h;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_h$7;->a:Lcom/kamcord/android/KC_h;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onDismiss(Landroid/content/DialogInterface;)V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_h$7;->a:Lcom/kamcord/android/KC_h;

    const/4 v1, 0x1

    invoke-static {v0, v1}, Lcom/kamcord/android/KC_h;->a(Lcom/kamcord/android/KC_h;Z)V

    return-void
.end method
