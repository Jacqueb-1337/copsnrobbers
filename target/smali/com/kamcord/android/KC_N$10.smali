.class final Lcom/kamcord/android/KC_N$10;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/view/View$OnClickListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/KC_N;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:I

.field private synthetic b:Lcom/kamcord/android/KC_N;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_N;I)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_N$10;->b:Lcom/kamcord/android/KC_N;

    iput p2, p0, Lcom/kamcord/android/KC_N$10;->a:I

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onClick(Landroid/view/View;)V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/KC_N$10;->b:Lcom/kamcord/android/KC_N;

    iget v1, p0, Lcom/kamcord/android/KC_N$10;->a:I

    invoke-static {v0, v1}, Lcom/kamcord/android/KC_N;->a(Lcom/kamcord/android/KC_N;I)Z

    move-result v0

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_N$10;->b:Lcom/kamcord/android/KC_N;

    iget-object v0, v0, Lcom/kamcord/android/KC_N;->T:[Lcom/kamcord/android/b/KC_e;

    iget v1, p0, Lcom/kamcord/android/KC_N$10;->a:I

    aget-object v0, v0, v1

    if-eqz v0, :cond_0

    invoke-static {}, La/a/a/c/KC_a;->a()Z

    move-result v0

    if-nez v0, :cond_1

    new-instance v0, Lcom/kamcord/android/KC_x;

    invoke-direct {v0}, Lcom/kamcord/android/KC_x;-><init>()V

    iget-object v1, p0, Lcom/kamcord/android/KC_N$10;->b:Lcom/kamcord/android/KC_N;

    invoke-virtual {v1}, Lcom/kamcord/android/KC_N;->h()La/a/a/a/KC_f;

    move-result-object v1

    invoke-virtual {v1}, La/a/a/a/KC_f;->getFragmentManager()Landroid/app/FragmentManager;

    move-result-object v1

    const/4 v2, 0x0

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/android/KC_x;->show(Landroid/app/FragmentManager;Ljava/lang/String;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N$10;->b:Lcom/kamcord/android/KC_N;

    iget v1, p0, Lcom/kamcord/android/KC_N$10;->a:I

    invoke-static {v0, v1}, Lcom/kamcord/android/KC_N;->a(Lcom/kamcord/android/KC_N;I)Z

    :cond_0
    :goto_0
    return-void

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/KC_N$10;->b:Lcom/kamcord/android/KC_N;

    iget-object v0, v0, Lcom/kamcord/android/KC_N;->T:[Lcom/kamcord/android/b/KC_e;

    iget v1, p0, Lcom/kamcord/android/KC_N$10;->a:I

    aget-object v0, v0, v1

    iget-object v1, p0, Lcom/kamcord/android/KC_N$10;->b:Lcom/kamcord/android/KC_N;

    invoke-virtual {v1}, Lcom/kamcord/android/KC_N;->h()La/a/a/a/KC_f;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/b/KC_e;->a(Landroid/content/Context;)V

    goto :goto_0
.end method
