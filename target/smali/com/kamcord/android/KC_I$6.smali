.class final Lcom/kamcord/android/KC_I$6;
.super Ljava/lang/Object;

# interfaces
.implements Ljava/lang/Runnable;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_I;->a(Ljava/lang/String;)V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:I

.field private synthetic b:Lcom/kamcord/android/KC_I;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_I;I)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_I$6;->b:Lcom/kamcord/android/KC_I;

    iput p2, p0, Lcom/kamcord/android/KC_I$6;->a:I

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final run()V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/KC_I$6;->b:Lcom/kamcord/android/KC_I;

    invoke-static {v0}, Lcom/kamcord/android/KC_I;->c(Lcom/kamcord/android/KC_I;)Ljava/util/List;

    move-result-object v0

    iget v1, p0, Lcom/kamcord/android/KC_I$6;->a:I

    invoke-interface {v0, v1}, Ljava/util/List;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Landroid/widget/TextView;

    iget-object v1, p0, Lcom/kamcord/android/KC_I$6;->b:Lcom/kamcord/android/KC_I;

    invoke-static {v1}, Lcom/kamcord/android/KC_I;->e(Lcom/kamcord/android/KC_I;)Ljava/util/List;

    move-result-object v1

    iget v2, p0, Lcom/kamcord/android/KC_I$6;->a:I

    invoke-interface {v1, v2}, Ljava/util/List;->get(I)Ljava/lang/Object;

    move-result-object v1

    check-cast v1, Lcom/kamcord/android/b/KC_e;

    invoke-virtual {v1}, Lcom/kamcord/android/b/KC_e;->c()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Landroid/widget/TextView;->setText(Ljava/lang/CharSequence;)V

    return-void
.end method
