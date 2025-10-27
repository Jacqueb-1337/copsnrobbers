.class final Lcom/kamcord/android/KamcordActivity$1;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/view/View$OnClickListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/KamcordActivity;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/KamcordActivity;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KamcordActivity;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KamcordActivity$1;->a:Lcom/kamcord/android/KamcordActivity;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onClick(Landroid/view/View;)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KamcordActivity$1;->a:Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v0}, Lcom/kamcord/android/KamcordActivity;->finish()V

    return-void
.end method
