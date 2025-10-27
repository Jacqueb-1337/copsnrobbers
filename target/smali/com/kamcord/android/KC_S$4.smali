.class final Lcom/kamcord/android/KC_S$4;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/view/View$OnClickListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_S;->D()Z
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/KC_S;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_S;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_S$4;->a:Lcom/kamcord/android/KC_S;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onClick(Landroid/view/View;)V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_S$4;->a:Lcom/kamcord/android/KC_S;

    iget-object v1, p0, Lcom/kamcord/android/KC_S$4;->a:Lcom/kamcord/android/KC_S;

    iget-object v1, v1, Lcom/kamcord/android/KC_S;->N:Lcom/kamcord/android/KC_w;

    invoke-virtual {v1}, Lcom/kamcord/android/KC_w;->c()Z

    move-result v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_S;->c(Z)V

    return-void
.end method
