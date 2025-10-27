.class final Lcom/kamcord/android/core/KC_O;
.super Lcom/kamcord/android/core/KC_M;


# direct methods
.method constructor <init>(Lcom/kamcord/android/core/KC_x;Lcom/kamcord/android/core/KC_V;)V
    .locals 0

    invoke-direct {p0, p1, p2}, Lcom/kamcord/android/core/KC_M;-><init>(Lcom/kamcord/android/core/KC_x;Lcom/kamcord/android/core/KC_V;)V

    return-void
.end method


# virtual methods
.method public final a(Lcom/kamcord/android/core/KC_F;)V
    .locals 1

    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_O;->n()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-virtual {p0, p1}, Lcom/kamcord/android/core/KC_O;->b(Lcom/kamcord/android/core/KC_F;)J

    :cond_0
    return-void
.end method

.method public final a(Lcom/kamcord/android/core/KC_y;J)Z
    .locals 4

    const/4 v3, 0x1

    if-eqz p1, :cond_1

    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_O;->g()Z

    move-result v0

    if-eqz v0, :cond_1

    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_O;->l()V

    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_O;->n()Z

    move-result v0

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_O;->b:Lcom/kamcord/android/core/KC_x;

    const/4 v1, 0x0

    iget-object v2, p0, Lcom/kamcord/android/core/KC_O;->c:Lcom/kamcord/android/core/KC_y;

    invoke-virtual {v0, v3, v1, p1, v2}, Lcom/kamcord/android/core/KC_x;->a(ZZLcom/kamcord/android/core/KC_y;Lcom/kamcord/android/core/KC_y;)V

    new-instance v0, Lcom/kamcord/android/core/KC_F;

    invoke-direct {v0, p2, p3}, Lcom/kamcord/android/core/KC_F;-><init>(J)V

    invoke-virtual {p0, v0}, Lcom/kamcord/android/core/KC_O;->a(Lcom/kamcord/android/core/KC_F;)V

    :cond_0
    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_O;->m()V

    :cond_1
    return v3
.end method

.method public final d()V
    .locals 0

    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_O;->h()V

    return-void
.end method

.method public final e()V
    .locals 0

    return-void
.end method
