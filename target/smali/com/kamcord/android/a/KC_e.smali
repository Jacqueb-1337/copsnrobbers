.class final Lcom/kamcord/android/a/KC_e;
.super Lcom/kamcord/android/a/KC_f;


# instance fields
.field private a:Lcom/kamcord/android/a/KC_b;


# direct methods
.method constructor <init>(Lcom/kamcord/android/a/KC_b;)V
    .locals 0

    invoke-direct {p0}, Lcom/kamcord/android/a/KC_f;-><init>()V

    iput-object p1, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    return-void
.end method


# virtual methods
.method public final a(Z)I
    .locals 1

    if-eqz p1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget v0, v0, Lcom/kamcord/android/a/KC_b;->h:I

    :goto_0
    return v0

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget v0, v0, Lcom/kamcord/android/a/KC_b;->i:I

    goto :goto_0
.end method

.method public final a(ZI)I
    .locals 1

    if-eqz p1, :cond_2

    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget v0, v0, Lcom/kamcord/android/a/KC_b;->l:I

    if-nez v0, :cond_1

    :cond_0
    :goto_0
    return p2

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget p2, v0, Lcom/kamcord/android/a/KC_b;->l:I

    goto :goto_0

    :cond_2
    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget v0, v0, Lcom/kamcord/android/a/KC_b;->p:I

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget p2, v0, Lcom/kamcord/android/a/KC_b;->p:I

    goto :goto_0
.end method

.method public final a(Lcom/kamcord/android/core/KC_f;)Ljava/util/List;
    .locals 2
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Lcom/kamcord/android/core/KC_f;",
            ")",
            "Ljava/util/List",
            "<",
            "Lcom/kamcord/android/core/KC_f;",
            ">;"
        }
    .end annotation

    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget-object v0, v0, Lcom/kamcord/android/a/KC_b;->a:Lcom/kamcord/android/core/KC_f;

    if-nez v0, :cond_0

    invoke-super {p0, p1}, Lcom/kamcord/android/a/KC_f;->a(Lcom/kamcord/android/core/KC_f;)Ljava/util/List;

    move-result-object v0

    :goto_0
    return-object v0

    :cond_0
    new-instance v0, Ljava/util/ArrayList;

    invoke-direct {v0}, Ljava/util/ArrayList;-><init>()V

    iget-object v1, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget-object v1, v1, Lcom/kamcord/android/a/KC_b;->a:Lcom/kamcord/android/core/KC_f;

    invoke-interface {v0, v1}, Ljava/util/List;->add(Ljava/lang/Object;)Z

    invoke-static {p1, v0}, Lcom/kamcord/android/a/KC_e;->a(Lcom/kamcord/android/core/KC_f;Ljava/util/List;)V

    goto :goto_0
.end method

.method public final b(Z)I
    .locals 1

    if-eqz p1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget v0, v0, Lcom/kamcord/android/a/KC_b;->j:I

    :goto_0
    return v0

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget v0, v0, Lcom/kamcord/android/a/KC_b;->k:I

    goto :goto_0
.end method

.method public final b(ZI)I
    .locals 1

    if-eqz p1, :cond_2

    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget v0, v0, Lcom/kamcord/android/a/KC_b;->m:I

    if-nez v0, :cond_1

    :cond_0
    :goto_0
    return p2

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget p2, v0, Lcom/kamcord/android/a/KC_b;->m:I

    goto :goto_0

    :cond_2
    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget v0, v0, Lcom/kamcord/android/a/KC_b;->q:I

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget p2, v0, Lcom/kamcord/android/a/KC_b;->q:I

    goto :goto_0
.end method

.method public final c(ZI)I
    .locals 1

    if-eqz p1, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget v0, v0, Lcom/kamcord/android/a/KC_b;->n:I

    if-nez v0, :cond_0

    div-int/lit8 v0, p2, 0x2

    :goto_0
    return v0

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget v0, v0, Lcom/kamcord/android/a/KC_b;->n:I

    goto :goto_0

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget v0, v0, Lcom/kamcord/android/a/KC_b;->r:I

    if-nez v0, :cond_2

    div-int/lit8 v0, p2, 0x2

    goto :goto_0

    :cond_2
    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget v0, v0, Lcom/kamcord/android/a/KC_b;->r:I

    goto :goto_0
.end method

.method public final c()Z
    .locals 2

    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    iget-object v1, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget v1, v1, Lcom/kamcord/android/a/KC_b;->u:I

    if-lt v0, v1, :cond_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public final d(ZI)I
    .locals 1

    if-eqz p1, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget v0, v0, Lcom/kamcord/android/a/KC_b;->o:I

    if-nez v0, :cond_0

    div-int/lit8 v0, p2, 0x2

    :goto_0
    return v0

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget v0, v0, Lcom/kamcord/android/a/KC_b;->o:I

    goto :goto_0

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget v0, v0, Lcom/kamcord/android/a/KC_b;->s:I

    if-nez v0, :cond_2

    div-int/lit8 v0, p2, 0x2

    goto :goto_0

    :cond_2
    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget v0, v0, Lcom/kamcord/android/a/KC_b;->s:I

    goto :goto_0
.end method

.method public final e()Z
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget-boolean v0, v0, Lcom/kamcord/android/a/KC_b;->v:Z

    return v0
.end method

.method public final f()Z
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget-boolean v0, v0, Lcom/kamcord/android/a/KC_b;->w:Z

    return v0
.end method

.method public final g()Z
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget-boolean v0, v0, Lcom/kamcord/android/a/KC_b;->g:Z

    return v0
.end method

.method public final h()Z
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget-boolean v0, v0, Lcom/kamcord/android/a/KC_b;->x:Z

    return v0
.end method

.method public final i()Z
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget-boolean v0, v0, Lcom/kamcord/android/a/KC_b;->f:Z

    return v0
.end method

.method public final j()Ljava/lang/String;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget-object v0, v0, Lcom/kamcord/android/a/KC_b;->b:Ljava/lang/String;

    return-object v0
.end method

.method public final k()Lcom/kamcord/android/core/KC_c;
    .locals 5

    const/4 v4, 0x0

    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget-object v0, v0, Lcom/kamcord/android/a/KC_b;->c:Ljava/lang/String;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget-object v0, v0, Lcom/kamcord/android/a/KC_b;->d:Ljava/lang/String;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget v0, v0, Lcom/kamcord/android/a/KC_b;->e:I

    const/4 v1, -0x1

    if-ne v0, v1, :cond_1

    :cond_0
    invoke-super {p0}, Lcom/kamcord/android/a/KC_f;->k()Lcom/kamcord/android/core/KC_c;

    move-result-object v0

    :goto_0
    return-object v0

    :cond_1
    invoke-virtual {p0}, Lcom/kamcord/android/a/KC_e;->c()Z

    move-result v0

    if-eqz v0, :cond_2

    new-instance v0, Lcom/kamcord/android/core/KC_c;

    iget-object v1, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget-object v1, v1, Lcom/kamcord/android/a/KC_b;->c:Ljava/lang/String;

    iget-object v2, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget-object v2, v2, Lcom/kamcord/android/a/KC_b;->d:Ljava/lang/String;

    const v3, 0x7f000789

    invoke-direct {v0, v1, v2, v3, v4}, Lcom/kamcord/android/core/KC_c;-><init>(Ljava/lang/String;Ljava/lang/String;ILcom/kamcord/android/core/KC_f;)V

    goto :goto_0

    :cond_2
    new-instance v0, Lcom/kamcord/android/core/KC_c;

    iget-object v1, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget-object v1, v1, Lcom/kamcord/android/a/KC_b;->c:Ljava/lang/String;

    iget-object v2, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget-object v2, v2, Lcom/kamcord/android/a/KC_b;->d:Ljava/lang/String;

    iget-object v3, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget v3, v3, Lcom/kamcord/android/a/KC_b;->e:I

    invoke-direct {v0, v1, v2, v3, v4}, Lcom/kamcord/android/core/KC_c;-><init>(Ljava/lang/String;Ljava/lang/String;ILcom/kamcord/android/core/KC_f;)V

    goto :goto_0
.end method

.method public final m()I
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget v0, v0, Lcom/kamcord/android/a/KC_b;->t:I

    return v0
.end method

.method public final n()D
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/a/KC_e;->a:Lcom/kamcord/android/a/KC_b;

    iget-wide v0, v0, Lcom/kamcord/android/a/KC_b;->y:D

    return-wide v0
.end method
