.class final La/a/a/a/KC_o$KC_a;
.super Ljava/lang/Object;
.source "SourceFile"

# interfaces
.implements La/a/a/b/KC_a$KC_a;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = La/a/a/a/KC_o;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x10
    name = "KC_a"
.end annotation

.annotation system Ldalvik/annotation/Signature;
    value = {
        "Ljava/lang/Object;",
        "La/a/a/b/KC_a$KC_a",
        "<",
        "Ljava/lang/Object;",
        ">;"
    }
.end annotation


# instance fields
.field final a:I

.field b:La/a/a/a/KC_h$KC_a;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "La/a/a/a/KC_h$KC_a",
            "<",
            "Ljava/lang/Object;",
            ">;"
        }
    .end annotation
.end field

.field c:La/a/a/b/KC_a;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "La/a/a/b/KC_a",
            "<",
            "Ljava/lang/Object;",
            ">;"
        }
    .end annotation
.end field

.field d:Z

.field e:Z

.field f:Z

.field g:Z

.field h:Z

.field i:Z

.field private j:Landroid/os/Bundle;

.field private k:Z

.field private l:Ljava/lang/Object;

.field private m:Z


# virtual methods
.method final a()V
    .locals 2

    .prologue
    const/4 v1, 0x0

    .line 314
    iput-boolean v1, p0, La/a/a/a/KC_o$KC_a;->e:Z

    .line 316
    iget-boolean v0, p0, La/a/a/a/KC_o$KC_a;->f:Z

    if-nez v0, :cond_0

    .line 317
    iget-object v0, p0, La/a/a/a/KC_o$KC_a;->c:La/a/a/b/KC_a;

    if-eqz v0, :cond_0

    iget-boolean v0, p0, La/a/a/a/KC_o$KC_a;->i:Z

    if-eqz v0, :cond_0

    .line 319
    iput-boolean v1, p0, La/a/a/a/KC_o$KC_a;->i:Z

    .line 320
    iget-object v0, p0, La/a/a/a/KC_o$KC_a;->c:La/a/a/b/KC_a;

    invoke-virtual {v0, p0}, La/a/a/b/KC_a;->a(La/a/a/b/KC_a$KC_a;)V

    .line 321
    iget-object v0, p0, La/a/a/a/KC_o$KC_a;->c:La/a/a/b/KC_a;

    invoke-virtual {v0}, La/a/a/b/KC_a;->b()V

    .line 324
    :cond_0
    return-void
.end method

.method public final a(Ljava/lang/String;Ljava/io/FileDescriptor;Ljava/io/PrintWriter;[Ljava/lang/String;)V
    .locals 4

    .prologue
    const/4 v3, 0x0

    .line 451
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mId="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget v0, p0, La/a/a/a/KC_o$KC_a;->a:I

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(I)V

    .line 452
    const-string v0, " mArgs="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-object v0, p0, La/a/a/a/KC_o$KC_a;->j:Landroid/os/Bundle;

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/Object;)V

    .line 453
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mCallbacks="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    invoke-virtual {p3, v3}, Ljava/io/PrintWriter;->println(Ljava/lang/Object;)V

    .line 454
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mLoader="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-object v0, p0, La/a/a/a/KC_o$KC_a;->c:La/a/a/b/KC_a;

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/Object;)V

    .line 455
    iget-object v0, p0, La/a/a/a/KC_o$KC_a;->c:La/a/a/b/KC_a;

    if-eqz v0, :cond_0

    .line 456
    iget-object v0, p0, La/a/a/a/KC_o$KC_a;->c:La/a/a/b/KC_a;

    new-instance v1, Ljava/lang/StringBuilder;

    invoke-direct {v1}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v1, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, "  "

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1, p3}, La/a/a/b/KC_a;->a(Ljava/lang/String;Ljava/io/PrintWriter;)V

    .line 458
    :cond_0
    iget-boolean v0, p0, La/a/a/a/KC_o$KC_a;->d:Z

    if-eqz v0, :cond_1

    .line 459
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mHaveData="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const/4 v0, 0x0

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Z)V

    .line 460
    const-string v0, "  mDeliveredData="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-boolean v0, p0, La/a/a/a/KC_o$KC_a;->d:Z

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Z)V

    .line 461
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mData="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    invoke-virtual {p3, v3}, Ljava/io/PrintWriter;->println(Ljava/lang/Object;)V

    .line 463
    :cond_1
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mStarted="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-boolean v0, p0, La/a/a/a/KC_o$KC_a;->e:Z

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Z)V

    .line 464
    const-string v0, " mReportNextStart="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-boolean v0, p0, La/a/a/a/KC_o$KC_a;->h:Z

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Z)V

    .line 465
    const-string v0, " mDestroyed="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-boolean v0, p0, La/a/a/a/KC_o$KC_a;->m:Z

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Z)V

    .line 466
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mRetaining="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-boolean v0, p0, La/a/a/a/KC_o$KC_a;->f:Z

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Z)V

    .line 467
    const-string v0, " mRetainingStarted="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-boolean v0, p0, La/a/a/a/KC_o$KC_a;->g:Z

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Z)V

    .line 468
    const-string v0, " mListenerRegistered="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-boolean v0, p0, La/a/a/a/KC_o$KC_a;->i:Z

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Z)V

    .line 469
    return-void
.end method

.method final b()V
    .locals 3

    .prologue
    const/4 v2, 0x0

    const/4 v1, 0x0

    .line 327
    const/4 v0, 0x1

    iput-boolean v0, p0, La/a/a/a/KC_o$KC_a;->m:Z

    .line 329
    iget-boolean v0, p0, La/a/a/a/KC_o$KC_a;->d:Z

    .line 330
    iput-boolean v1, p0, La/a/a/a/KC_o$KC_a;->d:Z

    .line 331
    iput-object v2, p0, La/a/a/a/KC_o$KC_a;->b:La/a/a/a/KC_h$KC_a;

    .line 347
    iput-object v2, p0, La/a/a/a/KC_o$KC_a;->l:Ljava/lang/Object;

    .line 348
    iput-boolean v1, p0, La/a/a/a/KC_o$KC_a;->k:Z

    .line 349
    iget-object v0, p0, La/a/a/a/KC_o$KC_a;->c:La/a/a/b/KC_a;

    if-eqz v0, :cond_1

    .line 350
    iget-boolean v0, p0, La/a/a/a/KC_o$KC_a;->i:Z

    if-eqz v0, :cond_0

    .line 351
    iput-boolean v1, p0, La/a/a/a/KC_o$KC_a;->i:Z

    .line 352
    iget-object v0, p0, La/a/a/a/KC_o$KC_a;->c:La/a/a/b/KC_a;

    invoke-virtual {v0, p0}, La/a/a/b/KC_a;->a(La/a/a/b/KC_a$KC_a;)V

    .line 354
    :cond_0
    iget-object v0, p0, La/a/a/a/KC_o$KC_a;->c:La/a/a/b/KC_a;

    invoke-virtual {v0}, La/a/a/b/KC_a;->c()V

    .line 356
    :cond_1
    return-void
.end method

.method public final toString()Ljava/lang/String;
    .locals 2

    .prologue
    .line 439
    new-instance v0, Ljava/lang/StringBuilder;

    const/16 v1, 0x40

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(I)V

    .line 440
    const-string v1, "LoaderInfo{"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 441
    invoke-static {p0}, Ljava/lang/System;->identityHashCode(Ljava/lang/Object;)I

    move-result v1

    invoke-static {v1}, Ljava/lang/Integer;->toHexString(I)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 442
    const-string v1, " #"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 443
    iget v1, p0, La/a/a/a/KC_o$KC_a;->a:I

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    .line 444
    const-string v1, " : "

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 445
    iget-object v1, p0, La/a/a/a/KC_o$KC_a;->c:La/a/a/b/KC_a;

    invoke-static {v1, v0}, La/a/a/c/KC_a;->a(Ljava/lang/Object;Ljava/lang/StringBuilder;)V

    .line 446
    const-string v1, "}}"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 447
    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    return-object v0
.end method
