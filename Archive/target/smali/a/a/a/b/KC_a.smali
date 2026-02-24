.class public final La/a/a/b/KC_a;
.super Ljava/lang/Object;
.source "SourceFile"


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        La/a/a/b/KC_a$KC_a;
    }
.end annotation

.annotation system Ldalvik/annotation/Signature;
    value = {
        "<D:",
        "Ljava/lang/Object;",
        ">",
        "Ljava/lang/Object;"
    }
.end annotation


# instance fields
.field private a:I

.field private b:La/a/a/b/KC_a$KC_a;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "La/a/a/b/KC_a$KC_a",
            "<TD;>;"
        }
    .end annotation
.end field

.field private c:Z

.field private d:Z

.field private e:Z

.field private f:Z

.field private g:Z


# virtual methods
.method public final a()V
    .locals 2

    .prologue
    const/4 v1, 0x0

    .line 195
    const/4 v0, 0x1

    iput-boolean v0, p0, La/a/a/b/KC_a;->c:Z

    .line 196
    iput-boolean v1, p0, La/a/a/b/KC_a;->e:Z

    .line 197
    iput-boolean v1, p0, La/a/a/b/KC_a;->d:Z

    .line 198
    return-void
.end method

.method public final a(ILa/a/a/b/KC_a$KC_a;)V
    .locals 2
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(I",
            "La/a/a/b/KC_a$KC_a",
            "<TD;>;)V"
        }
    .end annotation

    .prologue
    .line 130
    iget-object v0, p0, La/a/a/b/KC_a;->b:La/a/a/b/KC_a$KC_a;

    if-eqz v0, :cond_0

    .line 131
    new-instance v0, Ljava/lang/IllegalStateException;

    const-string v1, "There is already a listener registered"

    invoke-direct {v0, v1}, Ljava/lang/IllegalStateException;-><init>(Ljava/lang/String;)V

    throw v0

    .line 133
    :cond_0
    iput-object p2, p0, La/a/a/b/KC_a;->b:La/a/a/b/KC_a$KC_a;

    .line 134
    iput p1, p0, La/a/a/b/KC_a;->a:I

    .line 135
    return-void
.end method

.method public final a(La/a/a/b/KC_a$KC_a;)V
    .locals 2
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "La/a/a/b/KC_a$KC_a",
            "<TD;>;)V"
        }
    .end annotation

    .prologue
    .line 143
    iget-object v0, p0, La/a/a/b/KC_a;->b:La/a/a/b/KC_a$KC_a;

    if-nez v0, :cond_0

    .line 144
    new-instance v0, Ljava/lang/IllegalStateException;

    const-string v1, "No listener register"

    invoke-direct {v0, v1}, Ljava/lang/IllegalStateException;-><init>(Ljava/lang/String;)V

    throw v0

    .line 146
    :cond_0
    iget-object v0, p0, La/a/a/b/KC_a;->b:La/a/a/b/KC_a$KC_a;

    if-eq v0, p1, :cond_1

    .line 147
    new-instance v0, Ljava/lang/IllegalArgumentException;

    const-string v1, "Attempting to unregister the wrong listener"

    invoke-direct {v0, v1}, Ljava/lang/IllegalArgumentException;-><init>(Ljava/lang/String;)V

    throw v0

    .line 149
    :cond_1
    const/4 v0, 0x0

    iput-object v0, p0, La/a/a/b/KC_a;->b:La/a/a/b/KC_a$KC_a;

    .line 150
    return-void
.end method

.method public final a(Ljava/lang/String;Ljava/io/PrintWriter;)V
    .locals 2

    .prologue
    const/4 v1, 0x0

    .line 393
    invoke-virtual {p2, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mId="

    invoke-virtual {p2, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget v0, p0, La/a/a/b/KC_a;->a:I

    invoke-virtual {p2, v0}, Ljava/io/PrintWriter;->print(I)V

    .line 394
    const-string v0, " mListener="

    invoke-virtual {p2, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-object v0, p0, La/a/a/b/KC_a;->b:La/a/a/b/KC_a$KC_a;

    invoke-virtual {p2, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/Object;)V

    .line 395
    iget-boolean v0, p0, La/a/a/b/KC_a;->c:Z

    if-eqz v0, :cond_0

    .line 396
    invoke-virtual {p2, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mStarted="

    invoke-virtual {p2, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-boolean v0, p0, La/a/a/b/KC_a;->c:Z

    invoke-virtual {p2, v0}, Ljava/io/PrintWriter;->print(Z)V

    .line 397
    const-string v0, " mContentChanged="

    invoke-virtual {p2, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    invoke-virtual {p2, v1}, Ljava/io/PrintWriter;->print(Z)V

    .line 398
    const-string v0, " mProcessingChange="

    invoke-virtual {p2, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    invoke-virtual {p2, v1}, Ljava/io/PrintWriter;->println(Z)V

    .line 400
    :cond_0
    iget-boolean v0, p0, La/a/a/b/KC_a;->e:Z

    if-eqz v0, :cond_1

    .line 401
    invoke-virtual {p2, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mAbandoned="

    invoke-virtual {p2, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    invoke-virtual {p2, v1}, Ljava/io/PrintWriter;->print(Z)V

    .line 402
    const-string v0, " mReset="

    invoke-virtual {p2, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-boolean v0, p0, La/a/a/b/KC_a;->e:Z

    invoke-virtual {p2, v0}, Ljava/io/PrintWriter;->println(Z)V

    .line 404
    :cond_1
    return-void
.end method

.method public final b()V
    .locals 1

    .prologue
    .line 243
    const/4 v0, 0x0

    iput-boolean v0, p0, La/a/a/b/KC_a;->c:Z

    .line 244
    return-void
.end method

.method public final c()V
    .locals 2

    .prologue
    const/4 v1, 0x0

    .line 291
    const/4 v0, 0x1

    iput-boolean v0, p0, La/a/a/b/KC_a;->e:Z

    .line 293
    iput-boolean v1, p0, La/a/a/b/KC_a;->c:Z

    .line 294
    iput-boolean v1, p0, La/a/a/b/KC_a;->d:Z

    .line 295
    iput-boolean v1, p0, La/a/a/b/KC_a;->f:Z

    .line 296
    iput-boolean v1, p0, La/a/a/b/KC_a;->g:Z

    .line 297
    return-void
.end method

.method public final toString()Ljava/lang/String;
    .locals 2

    .prologue
    .line 376
    new-instance v0, Ljava/lang/StringBuilder;

    const/16 v1, 0x40

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(I)V

    .line 377
    invoke-static {p0, v0}, La/a/a/c/KC_a;->a(Ljava/lang/Object;Ljava/lang/StringBuilder;)V

    .line 378
    const-string v1, " id="

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 379
    iget v1, p0, La/a/a/b/KC_a;->a:I

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    .line 380
    const-string v1, "}"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 381
    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    return-object v0
.end method
