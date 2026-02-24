.class public La/a/a/a/KC_f;
.super Landroid/app/Activity;
.source "SourceFile"


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        La/a/a/a/KC_f$KC_a;,
        La/a/a/a/KC_f$KC_b;
    }
.end annotation


# instance fields
.field final a:Landroid/os/Handler;

.field final b:La/a/a/a/KC_i;

.field c:Z

.field d:Z

.field private e:La/a/a/a/KC_g;

.field private f:Z

.field private g:Z

.field private h:Z

.field private i:Z

.field private j:Z

.field private k:Z

.field private l:La/a/a/d/KC_e;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "La/a/a/d/KC_e",
            "<",
            "Ljava/lang/String;",
            "La/a/a/a/KC_o;",
            ">;"
        }
    .end annotation
.end field

.field private m:La/a/a/a/KC_o;


# direct methods
.method public constructor <init>()V
    .locals 1

    .prologue
    .line 76
    invoke-direct {p0}, Landroid/app/Activity;-><init>()V

    .line 87
    new-instance v0, La/a/a/a/KC_f$1;

    invoke-direct {v0, p0}, La/a/a/a/KC_f$1;-><init>(La/a/a/a/KC_f;)V

    iput-object v0, p0, La/a/a/a/KC_f;->a:Landroid/os/Handler;

    .line 106
    new-instance v0, La/a/a/a/KC_i;

    invoke-direct {v0}, La/a/a/a/KC_i;-><init>()V

    iput-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    .line 107
    new-instance v0, La/a/a/a/KC_f$2;

    invoke-direct {v0, p0}, La/a/a/a/KC_f$2;-><init>(La/a/a/a/KC_f;)V

    iput-object v0, p0, La/a/a/a/KC_f;->e:La/a/a/a/KC_g;

    .line 135
    return-void
.end method

.method private static a(Landroid/view/View;)Ljava/lang/String;
    .locals 7

    .prologue
    const/16 v3, 0x56

    const/16 v1, 0x46

    const/16 v6, 0x2c

    const/16 v5, 0x20

    const/16 v2, 0x2e

    .line 696
    new-instance v4, Ljava/lang/StringBuilder;

    const/16 v0, 0x80

    invoke-direct {v4, v0}, Ljava/lang/StringBuilder;-><init>(I)V

    .line 697
    invoke-virtual {p0}, Ljava/lang/Object;->getClass()Ljava/lang/Class;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/Class;->getName()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 698
    const/16 v0, 0x7b

    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 699
    invoke-static {p0}, Ljava/lang/System;->identityHashCode(Ljava/lang/Object;)I

    move-result v0

    invoke-static {v0}, Ljava/lang/Integer;->toHexString(I)Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 700
    invoke-virtual {v4, v5}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 701
    invoke-virtual {p0}, Landroid/view/View;->getVisibility()I

    move-result v0

    sparse-switch v0, :sswitch_data_0

    .line 705
    invoke-virtual {v4, v2}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 707
    :goto_0
    invoke-virtual {p0}, Landroid/view/View;->isFocusable()Z

    move-result v0

    if-eqz v0, :cond_2

    move v0, v1

    :goto_1
    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 708
    invoke-virtual {p0}, Landroid/view/View;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_3

    const/16 v0, 0x45

    :goto_2
    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 709
    invoke-virtual {p0}, Landroid/view/View;->willNotDraw()Z

    move-result v0

    if-eqz v0, :cond_4

    move v0, v2

    :goto_3
    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 710
    invoke-virtual {p0}, Landroid/view/View;->isHorizontalScrollBarEnabled()Z

    move-result v0

    if-eqz v0, :cond_5

    const/16 v0, 0x48

    :goto_4
    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 711
    invoke-virtual {p0}, Landroid/view/View;->isVerticalScrollBarEnabled()Z

    move-result v0

    if-eqz v0, :cond_6

    move v0, v3

    :goto_5
    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 712
    invoke-virtual {p0}, Landroid/view/View;->isClickable()Z

    move-result v0

    if-eqz v0, :cond_7

    const/16 v0, 0x43

    :goto_6
    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 713
    invoke-virtual {p0}, Landroid/view/View;->isLongClickable()Z

    move-result v0

    if-eqz v0, :cond_8

    const/16 v0, 0x4c

    :goto_7
    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 714
    invoke-virtual {v4, v5}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 715
    invoke-virtual {p0}, Landroid/view/View;->isFocused()Z

    move-result v0

    if-eqz v0, :cond_9

    :goto_8
    invoke-virtual {v4, v1}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 716
    invoke-virtual {p0}, Landroid/view/View;->isSelected()Z

    move-result v0

    if-eqz v0, :cond_a

    const/16 v0, 0x53

    :goto_9
    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 717
    invoke-virtual {p0}, Landroid/view/View;->isPressed()Z

    move-result v0

    if-eqz v0, :cond_0

    const/16 v2, 0x50

    :cond_0
    invoke-virtual {v4, v2}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 718
    invoke-virtual {v4, v5}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 719
    invoke-virtual {p0}, Landroid/view/View;->getLeft()I

    move-result v0

    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    .line 720
    invoke-virtual {v4, v6}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 721
    invoke-virtual {p0}, Landroid/view/View;->getTop()I

    move-result v0

    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    .line 722
    const/16 v0, 0x2d

    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 723
    invoke-virtual {p0}, Landroid/view/View;->getRight()I

    move-result v0

    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    .line 724
    invoke-virtual {v4, v6}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 725
    invoke-virtual {p0}, Landroid/view/View;->getBottom()I

    move-result v0

    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    .line 726
    invoke-virtual {p0}, Landroid/view/View;->getId()I

    move-result v1

    .line 727
    const/4 v0, -0x1

    if-eq v1, v0, :cond_1

    .line 728
    const-string v0, " #"

    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 729
    invoke-static {v1}, Ljava/lang/Integer;->toHexString(I)Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 730
    invoke-virtual {p0}, Landroid/view/View;->getResources()Landroid/content/res/Resources;

    move-result-object v2

    .line 731
    if-eqz v1, :cond_1

    if-eqz v2, :cond_1

    .line 734
    const/high16 v0, -0x1000000

    and-int/2addr v0, v1

    sparse-switch v0, :sswitch_data_1

    .line 742
    :try_start_0
    invoke-virtual {v2, v1}, Landroid/content/res/Resources;->getResourcePackageName(I)Ljava/lang/String;

    move-result-object v0

    .line 745
    :goto_a
    invoke-virtual {v2, v1}, Landroid/content/res/Resources;->getResourceTypeName(I)Ljava/lang/String;

    move-result-object v3

    .line 746
    invoke-virtual {v2, v1}, Landroid/content/res/Resources;->getResourceEntryName(I)Ljava/lang/String;

    move-result-object v1

    .line 747
    const-string v2, " "

    invoke-virtual {v4, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 748
    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 749
    const-string v0, ":"

    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 750
    invoke-virtual {v4, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 751
    const-string v0, "/"

    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 752
    invoke-virtual {v4, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;
    :try_end_0
    .catch Landroid/content/res/Resources$NotFoundException; {:try_start_0 .. :try_end_0} :catch_0

    .line 757
    :cond_1
    :goto_b
    const-string v0, "}"

    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 758
    invoke-virtual {v4}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    return-object v0

    .line 702
    :sswitch_0
    invoke-virtual {v4, v3}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    goto/16 :goto_0

    .line 703
    :sswitch_1
    const/16 v0, 0x49

    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    goto/16 :goto_0

    .line 704
    :sswitch_2
    const/16 v0, 0x47

    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    goto/16 :goto_0

    :cond_2
    move v0, v2

    .line 707
    goto/16 :goto_1

    :cond_3
    move v0, v2

    .line 708
    goto/16 :goto_2

    .line 709
    :cond_4
    const/16 v0, 0x44

    goto/16 :goto_3

    :cond_5
    move v0, v2

    .line 710
    goto/16 :goto_4

    :cond_6
    move v0, v2

    .line 711
    goto/16 :goto_5

    :cond_7
    move v0, v2

    .line 712
    goto/16 :goto_6

    :cond_8
    move v0, v2

    .line 713
    goto/16 :goto_7

    :cond_9
    move v1, v2

    .line 715
    goto/16 :goto_8

    :cond_a
    move v0, v2

    .line 716
    goto/16 :goto_9

    .line 736
    :sswitch_3
    :try_start_1
    const-string v0, "app"

    goto :goto_a

    .line 739
    :sswitch_4
    const-string v0, "android"
    :try_end_1
    .catch Landroid/content/res/Resources$NotFoundException; {:try_start_1 .. :try_end_1} :catch_0

    goto :goto_a

    :catch_0
    move-exception v0

    goto :goto_b

    .line 701
    nop

    :sswitch_data_0
    .sparse-switch
        0x0 -> :sswitch_0
        0x4 -> :sswitch_1
        0x8 -> :sswitch_2
    .end sparse-switch

    .line 734
    :sswitch_data_1
    .sparse-switch
        0x1000000 -> :sswitch_4
        0x7f000000 -> :sswitch_3
    .end sparse-switch
.end method

.method private a(Ljava/lang/String;Ljava/io/PrintWriter;Landroid/view/View;)V
    .locals 4

    .prologue
    .line 762
    invoke-virtual {p2, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    .line 763
    if-nez p3, :cond_1

    .line 764
    const-string v0, "null"

    invoke-virtual {p2, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/String;)V

    .line 780
    :cond_0
    return-void

    .line 767
    :cond_1
    invoke-static {p3}, La/a/a/a/KC_f;->a(Landroid/view/View;)Ljava/lang/String;

    move-result-object v0

    invoke-virtual {p2, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/String;)V

    .line 768
    instance-of v0, p3, Landroid/view/ViewGroup;

    if-eqz v0, :cond_0

    .line 771
    check-cast p3, Landroid/view/ViewGroup;

    .line 772
    invoke-virtual {p3}, Landroid/view/ViewGroup;->getChildCount()I

    move-result v1

    .line 773
    if-lez v1, :cond_0

    .line 776
    new-instance v0, Ljava/lang/StringBuilder;

    invoke-direct {v0}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v0, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v2, "  "

    invoke-virtual {v0, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    .line 777
    const/4 v0, 0x0

    :goto_0
    if-ge v0, v1, :cond_0

    .line 778
    invoke-virtual {p3, v0}, Landroid/view/ViewGroup;->getChildAt(I)Landroid/view/View;

    move-result-object v3

    invoke-direct {p0, v2, p2, v3}, La/a/a/a/KC_f;->a(Ljava/lang/String;Ljava/io/PrintWriter;Landroid/view/View;)V

    .line 777
    add-int/lit8 v0, v0, 0x1

    goto :goto_0
.end method


# virtual methods
.method final a(Ljava/lang/String;ZZ)La/a/a/a/KC_o;
    .locals 2

    .prologue
    .line 886
    iget-object v0, p0, La/a/a/a/KC_f;->l:La/a/a/d/KC_e;

    if-nez v0, :cond_0

    .line 887
    new-instance v0, La/a/a/d/KC_e;

    invoke-direct {v0}, La/a/a/d/KC_e;-><init>()V

    iput-object v0, p0, La/a/a/a/KC_f;->l:La/a/a/d/KC_e;

    .line 889
    :cond_0
    iget-object v0, p0, La/a/a/a/KC_f;->l:La/a/a/d/KC_e;

    invoke-virtual {v0, p1}, La/a/a/d/KC_e;->get(Ljava/lang/Object;)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/a/KC_o;

    .line 890
    if-nez v0, :cond_2

    .line 891
    if-eqz p3, :cond_1

    .line 892
    new-instance v0, La/a/a/a/KC_o;

    invoke-direct {v0, p1, p0, p2}, La/a/a/a/KC_o;-><init>(Ljava/lang/String;La/a/a/a/KC_f;Z)V

    .line 893
    iget-object v1, p0, La/a/a/a/KC_f;->l:La/a/a/d/KC_e;

    invoke-virtual {v1, p1, v0}, La/a/a/d/KC_e;->put(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;

    .line 898
    :cond_1
    :goto_0
    return-object v0

    .line 896
    :cond_2
    invoke-virtual {v0, p0}, La/a/a/a/KC_o;->a(La/a/a/a/KC_f;)V

    goto :goto_0
.end method

.method final a(Ljava/lang/String;)V
    .locals 2

    .prologue
    .line 860
    iget-object v0, p0, La/a/a/a/KC_f;->l:La/a/a/d/KC_e;

    if-eqz v0, :cond_0

    .line 861
    iget-object v0, p0, La/a/a/a/KC_f;->l:La/a/a/d/KC_e;

    invoke-virtual {v0, p1}, La/a/a/d/KC_e;->get(Ljava/lang/Object;)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/a/KC_o;

    .line 862
    if-eqz v0, :cond_0

    iget-boolean v1, v0, La/a/a/a/KC_o;->e:Z

    if-nez v1, :cond_0

    .line 863
    invoke-virtual {v0}, La/a/a/a/KC_o;->h()V

    .line 864
    iget-object v0, p0, La/a/a/a/KC_f;->l:La/a/a/d/KC_e;

    invoke-virtual {v0, p1}, La/a/a/d/KC_e;->remove(Ljava/lang/Object;)Ljava/lang/Object;

    .line 867
    :cond_0
    return-void
.end method

.method final a(Z)V
    .locals 2

    .prologue
    const/4 v1, 0x1

    .line 783
    iget-boolean v0, p0, La/a/a/a/KC_f;->h:Z

    if-nez v0, :cond_1

    .line 784
    iput-boolean v1, p0, La/a/a/a/KC_f;->h:Z

    .line 785
    iput-boolean p1, p0, La/a/a/a/KC_f;->d:Z

    .line 786
    iget-object v0, p0, La/a/a/a/KC_f;->a:Landroid/os/Handler;

    invoke-virtual {v0, v1}, Landroid/os/Handler;->removeMessages(I)V

    .line 787
    iget-boolean v0, p0, La/a/a/a/KC_f;->k:Z

    if-eqz v0, :cond_0

    const/4 v0, 0x0

    iput-boolean v0, p0, La/a/a/a/KC_f;->k:Z

    iget-object v0, p0, La/a/a/a/KC_f;->m:La/a/a/a/KC_o;

    if-eqz v0, :cond_0

    iget-boolean v0, p0, La/a/a/a/KC_f;->d:Z

    if-nez v0, :cond_2

    iget-object v0, p0, La/a/a/a/KC_f;->m:La/a/a/a/KC_o;

    invoke-virtual {v0}, La/a/a/a/KC_o;->c()V

    :cond_0
    :goto_0
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->n()V

    .line 789
    :cond_1
    return-void

    .line 787
    :cond_2
    iget-object v0, p0, La/a/a/a/KC_f;->m:La/a/a/a/KC_o;

    invoke-virtual {v0}, La/a/a/a/KC_o;->d()V

    goto :goto_0
.end method

.method public dump(Ljava/lang/String;Ljava/io/FileDescriptor;Ljava/io/PrintWriter;[Ljava/lang/String;)V
    .locals 3
    .param p1, "prefix"    # Ljava/lang/String;
    .param p2, "fd"    # Ljava/io/FileDescriptor;
    .param p3, "writer"    # Ljava/io/PrintWriter;
    .param p4, "args"    # [Ljava/lang/String;

    .prologue
    .line 669
    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    .line 673
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "Local FragmentActivity "

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    .line 674
    invoke-static {p0}, Ljava/lang/System;->identityHashCode(Ljava/lang/Object;)I

    move-result v0

    invoke-static {v0}, Ljava/lang/Integer;->toHexString(I)Ljava/lang/String;

    move-result-object v0

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    .line 675
    const-string v0, " State:"

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/String;)V

    .line 676
    new-instance v0, Ljava/lang/StringBuilder;

    invoke-direct {v0}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v0, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, "  "

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    .line 677
    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v1, "mCreated="

    invoke-virtual {p3, v1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    .line 678
    iget-boolean v1, p0, La/a/a/a/KC_f;->f:Z

    invoke-virtual {p3, v1}, Ljava/io/PrintWriter;->print(Z)V

    const-string v1, "mResumed="

    invoke-virtual {p3, v1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    .line 679
    iget-boolean v1, p0, La/a/a/a/KC_f;->g:Z

    invoke-virtual {p3, v1}, Ljava/io/PrintWriter;->print(Z)V

    const-string v1, " mStopped="

    invoke-virtual {p3, v1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    .line 680
    iget-boolean v1, p0, La/a/a/a/KC_f;->c:Z

    invoke-virtual {p3, v1}, Ljava/io/PrintWriter;->print(Z)V

    const-string v1, " mReallyStopped="

    invoke-virtual {p3, v1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    .line 681
    iget-boolean v1, p0, La/a/a/a/KC_f;->h:Z

    invoke-virtual {p3, v1}, Ljava/io/PrintWriter;->println(Z)V

    .line 682
    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mLoadersStarted="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    .line 683
    iget-boolean v0, p0, La/a/a/a/KC_f;->k:Z

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Z)V

    .line 684
    iget-object v0, p0, La/a/a/a/KC_f;->m:La/a/a/a/KC_o;

    if-eqz v0, :cond_0

    .line 685
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "Loader Manager "

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    .line 686
    iget-object v0, p0, La/a/a/a/KC_f;->m:La/a/a/a/KC_o;

    invoke-static {v0}, Ljava/lang/System;->identityHashCode(Ljava/lang/Object;)I

    move-result v0

    invoke-static {v0}, Ljava/lang/Integer;->toHexString(I)Ljava/lang/String;

    move-result-object v0

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    .line 687
    const-string v0, ":"

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/String;)V

    .line 688
    iget-object v0, p0, La/a/a/a/KC_f;->m:La/a/a/a/KC_o;

    new-instance v1, Ljava/lang/StringBuilder;

    invoke-direct {v1}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v1, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, "  "

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1, p2, p3, p4}, La/a/a/a/KC_o;->a(Ljava/lang/String;Ljava/io/FileDescriptor;Ljava/io/PrintWriter;[Ljava/lang/String;)V

    .line 690
    :cond_0
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0, p1, p2, p3, p4}, La/a/a/a/KC_i;->a(Ljava/lang/String;Ljava/io/FileDescriptor;Ljava/io/PrintWriter;[Ljava/lang/String;)V

    .line 691
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "View Hierarchy:"

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/String;)V

    .line 692
    new-instance v0, Ljava/lang/StringBuilder;

    invoke-direct {v0}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v0, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, "  "

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {p0}, La/a/a/a/KC_f;->getWindow()Landroid/view/Window;

    move-result-object v1

    invoke-virtual {v1}, Landroid/view/Window;->getDecorView()Landroid/view/View;

    move-result-object v1

    invoke-direct {p0, v0, p3, v1}, La/a/a/a/KC_f;->a(Ljava/lang/String;Ljava/io/PrintWriter;Landroid/view/View;)V

    .line 693
    return-void
.end method

.method public getLastCustomNonConfigurationInstance()Ljava/lang/Object;
    .locals 1

    .prologue
    .line 633
    invoke-virtual {p0}, La/a/a/a/KC_f;->getLastNonConfigurationInstance()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/a/KC_f$KC_b;

    .line 635
    if-eqz v0, :cond_0

    iget-object v0, v0, La/a/a/a/KC_f$KC_b;->a:Ljava/lang/Object;

    :goto_0
    return-object v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public getSupportFragmentManager()La/a/a/a/KC_h;
    .locals 1

    .prologue
    .line 828
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    return-object v0
.end method

.method public getSupportLoaderManager()La/a/a/a/KC_n;
    .locals 3

    .prologue
    const/4 v2, 0x1

    .line 877
    iget-object v0, p0, La/a/a/a/KC_f;->m:La/a/a/a/KC_o;

    if-eqz v0, :cond_0

    .line 878
    iget-object v0, p0, La/a/a/a/KC_f;->m:La/a/a/a/KC_o;

    .line 882
    :goto_0
    return-object v0

    .line 880
    :cond_0
    iput-boolean v2, p0, La/a/a/a/KC_f;->j:Z

    .line 881
    const-string v0, "(root)"

    iget-boolean v1, p0, La/a/a/a/KC_f;->k:Z

    invoke-virtual {p0, v0, v1, v2}, La/a/a/a/KC_f;->a(Ljava/lang/String;ZZ)La/a/a/a/KC_o;

    move-result-object v0

    iput-object v0, p0, La/a/a/a/KC_f;->m:La/a/a/a/KC_o;

    .line 882
    iget-object v0, p0, La/a/a/a/KC_f;->m:La/a/a/a/KC_o;

    goto :goto_0
.end method

.method protected onActivityResult(IILandroid/content/Intent;)V
    .locals 3
    .param p1, "requestCode"    # I
    .param p2, "resultCode"    # I
    .param p3, "data"    # Landroid/content/Intent;

    .prologue
    .line 153
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->g()V

    .line 154
    shr-int/lit8 v0, p1, 0x10

    .line 155
    if-eqz v0, :cond_3

    .line 156
    add-int/lit8 v0, v0, -0x1

    .line 157
    iget-object v1, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    iget-object v1, v1, La/a/a/a/KC_i;->b:Ljava/util/ArrayList;

    if-eqz v1, :cond_0

    if-ltz v0, :cond_0

    iget-object v1, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    iget-object v1, v1, La/a/a/a/KC_i;->b:Ljava/util/ArrayList;

    invoke-virtual {v1}, Ljava/util/ArrayList;->size()I

    move-result v1

    if-lt v0, v1, :cond_1

    .line 158
    :cond_0
    const-string v0, "FragmentActivity"

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Activity result fragment index out of range: 0x"

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-static {p1}, Ljava/lang/Integer;->toHexString(I)Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v0, v1}, Landroid/util/Log;->w(Ljava/lang/String;Ljava/lang/String;)I

    .line 173
    :goto_0
    return-void

    .line 162
    :cond_1
    iget-object v1, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    iget-object v1, v1, La/a/a/a/KC_i;->b:Ljava/util/ArrayList;

    invoke-virtual {v1, v0}, Ljava/util/ArrayList;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/a/KC_e;

    .line 163
    if-nez v0, :cond_2

    .line 164
    const-string v0, "FragmentActivity"

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Activity result no fragment exists for index: 0x"

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-static {p1}, Ljava/lang/Integer;->toHexString(I)Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v0, v1}, Landroid/util/Log;->w(Ljava/lang/String;Ljava/lang/String;)I

    goto :goto_0

    .line 167
    :cond_2
    const v1, 0xffff

    and-int/2addr v1, p1

    invoke-virtual {v0, v1, p2, p3}, La/a/a/a/KC_e;->a(IILandroid/content/Intent;)V

    goto :goto_0

    .line 172
    :cond_3
    invoke-super {p0, p1, p2, p3}, Landroid/app/Activity;->onActivityResult(IILandroid/content/Intent;)V

    goto :goto_0
.end method

.method public onAttachFragment(La/a/a/a/KC_e;)V
    .locals 0
    .param p1, "fragment"    # La/a/a/a/KC_e;

    .prologue
    .line 821
    return-void
.end method

.method public onBackPressed()V
    .locals 1

    .prologue
    .line 180
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->c()Z

    move-result v0

    if-nez v0, :cond_0

    .line 181
    invoke-virtual {p0}, La/a/a/a/KC_f;->finish()V

    .line 183
    :cond_0
    return-void
.end method

.method public onConfigurationChanged(Landroid/content/res/Configuration;)V
    .locals 1
    .param p1, "newConfig"    # Landroid/content/res/Configuration;

    .prologue
    .line 190
    invoke-super {p0, p1}, Landroid/app/Activity;->onConfigurationChanged(Landroid/content/res/Configuration;)V

    .line 191
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0, p1}, La/a/a/a/KC_i;->a(Landroid/content/res/Configuration;)V

    .line 192
    return-void
.end method

.method protected onCreate(Landroid/os/Bundle;)V
    .locals 4
    .param p1, "savedInstanceState"    # Landroid/os/Bundle;

    .prologue
    const/4 v1, 0x0

    .line 199
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    iget-object v2, p0, La/a/a/a/KC_f;->e:La/a/a/a/KC_g;

    invoke-virtual {v0, p0, v2, v1}, La/a/a/a/KC_i;->a(La/a/a/a/KC_f;La/a/a/a/KC_g;La/a/a/a/KC_e;)V

    .line 201
    invoke-virtual {p0}, La/a/a/a/KC_f;->getLayoutInflater()Landroid/view/LayoutInflater;

    move-result-object v0

    invoke-virtual {v0}, Landroid/view/LayoutInflater;->getFactory()Landroid/view/LayoutInflater$Factory;

    move-result-object v0

    if-nez v0, :cond_0

    .line 202
    invoke-virtual {p0}, La/a/a/a/KC_f;->getLayoutInflater()Landroid/view/LayoutInflater;

    move-result-object v0

    invoke-virtual {v0, p0}, Landroid/view/LayoutInflater;->setFactory(Landroid/view/LayoutInflater$Factory;)V

    .line 205
    :cond_0
    invoke-super {p0, p1}, Landroid/app/Activity;->onCreate(Landroid/os/Bundle;)V

    .line 207
    invoke-virtual {p0}, La/a/a/a/KC_f;->getLastNonConfigurationInstance()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/a/KC_f$KC_b;

    .line 209
    if-eqz v0, :cond_1

    .line 210
    iget-object v2, v0, La/a/a/a/KC_f$KC_b;->c:La/a/a/d/KC_e;

    iput-object v2, p0, La/a/a/a/KC_f;->l:La/a/a/d/KC_e;

    .line 212
    :cond_1
    if-eqz p1, :cond_2

    .line 213
    const-string v2, "android:support:fragments"

    invoke-virtual {p1, v2}, Landroid/os/Bundle;->getParcelable(Ljava/lang/String;)Landroid/os/Parcelable;

    move-result-object v2

    .line 214
    iget-object v3, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    if-eqz v0, :cond_3

    iget-object v0, v0, La/a/a/a/KC_f$KC_b;->b:Ljava/util/ArrayList;

    :goto_0
    invoke-virtual {v3, v2, v0}, La/a/a/a/KC_i;->a(Landroid/os/Parcelable;Ljava/util/ArrayList;)V

    .line 216
    :cond_2
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->h()V

    .line 217
    return-void

    :cond_3
    move-object v0, v1

    .line 214
    goto :goto_0
.end method

.method public onCreatePanelMenu(ILandroid/view/Menu;)Z
    .locals 3
    .param p1, "featureId"    # I
    .param p2, "menu"    # Landroid/view/Menu;

    .prologue
    .line 224
    if-nez p1, :cond_1

    .line 225
    invoke-super {p0, p1, p2}, Landroid/app/Activity;->onCreatePanelMenu(ILandroid/view/Menu;)Z

    move-result v0

    .line 226
    iget-object v1, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {p0}, La/a/a/a/KC_f;->getMenuInflater()Landroid/view/MenuInflater;

    move-result-object v2

    invoke-virtual {v1, p2, v2}, La/a/a/a/KC_i;->a(Landroid/view/Menu;Landroid/view/MenuInflater;)Z

    move-result v1

    or-int/2addr v0, v1

    .line 227
    sget v1, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v2, 0xb

    if-lt v1, v2, :cond_0

    .line 235
    :goto_0
    return v0

    .line 233
    :cond_0
    const/4 v0, 0x1

    goto :goto_0

    .line 235
    :cond_1
    invoke-super {p0, p1, p2}, Landroid/app/Activity;->onCreatePanelMenu(ILandroid/view/Menu;)Z

    move-result v0

    goto :goto_0
.end method

.method public onCreateView(Ljava/lang/String;Landroid/content/Context;Landroid/util/AttributeSet;)Landroid/view/View;
    .locals 8
    .param p1, "name"    # Ljava/lang/String;
    .param p2, "context"    # Landroid/content/Context;
    .param p3, "attrs"    # Landroid/util/AttributeSet;

    .prologue
    const/4 v1, 0x0

    const/4 v7, -0x1

    const/4 v6, 0x1

    const/4 v3, 0x0

    .line 243
    const-string v0, "fragment"

    invoke-virtual {v0, p1}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-nez v0, :cond_0

    .line 244
    invoke-super {p0, p1, p2, p3}, Landroid/app/Activity;->onCreateView(Ljava/lang/String;Landroid/content/Context;Landroid/util/AttributeSet;)Landroid/view/View;

    move-result-object v0

    .line 324
    :goto_0
    return-object v0

    .line 247
    :cond_0
    const-string v0, "class"

    invoke-interface {p3, v1, v0}, Landroid/util/AttributeSet;->getAttributeValue(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    .line 248
    sget-object v2, La/a/a/a/KC_f$KC_a;->a:[I

    invoke-virtual {p2, p3, v2}, Landroid/content/Context;->obtainStyledAttributes(Landroid/util/AttributeSet;[I)Landroid/content/res/TypedArray;

    move-result-object v4

    .line 249
    if-nez v0, :cond_1

    .line 250
    invoke-virtual {v4, v3}, Landroid/content/res/TypedArray;->getString(I)Ljava/lang/String;

    move-result-object v0

    .line 252
    :cond_1
    invoke-virtual {v4, v6, v7}, Landroid/content/res/TypedArray;->getResourceId(II)I

    move-result v2

    .line 253
    const/4 v5, 0x2

    invoke-virtual {v4, v5}, Landroid/content/res/TypedArray;->getString(I)Ljava/lang/String;

    move-result-object v5

    .line 254
    invoke-virtual {v4}, Landroid/content/res/TypedArray;->recycle()V

    .line 256
    invoke-static {p0, v0}, La/a/a/a/KC_e;->b(Landroid/content/Context;Ljava/lang/String;)Z

    move-result v4

    if-nez v4, :cond_2

    .line 259
    invoke-super {p0, p1, p2, p3}, Landroid/app/Activity;->onCreateView(Ljava/lang/String;Landroid/content/Context;Landroid/util/AttributeSet;)Landroid/view/View;

    move-result-object v0

    goto :goto_0

    .line 262
    :cond_2
    if-eq v2, v7, :cond_3

    iget-object v1, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v1, v2}, La/a/a/a/KC_i;->a(I)La/a/a/a/KC_e;

    move-result-object v1

    .line 273
    :cond_3
    if-nez v1, :cond_4

    if-eqz v5, :cond_4

    .line 274
    iget-object v1, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v1, v5}, La/a/a/a/KC_i;->a(Ljava/lang/String;)La/a/a/a/KC_e;

    move-result-object v1

    .line 276
    :cond_4
    if-nez v1, :cond_5

    .line 277
    iget-object v1, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v1, v3}, La/a/a/a/KC_i;->a(I)La/a/a/a/KC_e;

    move-result-object v1

    .line 280
    :cond_5
    if-nez v1, :cond_7

    .line 284
    invoke-static {p0, v0}, La/a/a/a/KC_e;->a(Landroid/content/Context;Ljava/lang/String;)La/a/a/a/KC_e;

    move-result-object v4

    .line 285
    iput-boolean v6, v4, La/a/a/a/KC_e;->o:Z

    .line 286
    if-eqz v2, :cond_6

    move v1, v2

    :goto_1
    iput v1, v4, La/a/a/a/KC_e;->w:I

    .line 287
    iput v3, v4, La/a/a/a/KC_e;->x:I

    .line 288
    iput-object v5, v4, La/a/a/a/KC_e;->y:Ljava/lang/String;

    .line 289
    iput-boolean v6, v4, La/a/a/a/KC_e;->p:Z

    .line 290
    iget-object v1, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    iput-object v1, v4, La/a/a/a/KC_e;->s:La/a/a/a/KC_i;

    .line 291
    iget-object v1, v4, La/a/a/a/KC_e;->d:Landroid/os/Bundle;

    invoke-virtual {v4}, La/a/a/a/KC_e;->k()V

    .line 292
    iget-object v1, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v1, v4, v6}, La/a/a/a/KC_i;->a(La/a/a/a/KC_e;Z)V

    move-object v1, v4

    .line 314
    :goto_2
    iget-object v3, v1, La/a/a/a/KC_e;->H:Landroid/view/View;

    if-nez v3, :cond_a

    .line 315
    new-instance v1, Ljava/lang/IllegalStateException;

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Fragment "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v2, " did not create a view."

    invoke-virtual {v0, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-direct {v1, v0}, Ljava/lang/IllegalStateException;-><init>(Ljava/lang/String;)V

    throw v1

    :cond_6
    move v1, v3

    .line 286
    goto :goto_1

    .line 294
    :cond_7
    iget-boolean v4, v1, La/a/a/a/KC_e;->p:Z

    if-eqz v4, :cond_8

    .line 297
    new-instance v1, Ljava/lang/IllegalArgumentException;

    new-instance v4, Ljava/lang/StringBuilder;

    invoke-direct {v4}, Ljava/lang/StringBuilder;-><init>()V

    invoke-interface {p3}, Landroid/util/AttributeSet;->getPositionDescription()Ljava/lang/String;

    move-result-object v6

    invoke-virtual {v4, v6}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    const-string v6, ": Duplicate id 0x"

    invoke-virtual {v4, v6}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v4

    invoke-static {v2}, Ljava/lang/Integer;->toHexString(I)Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v4, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v4, ", tag "

    invoke-virtual {v2, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2, v5}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v4, ", or parent id 0x"

    invoke-virtual {v2, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-static {v3}, Ljava/lang/Integer;->toHexString(I)Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, " with another fragment for "

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-direct {v1, v0}, Ljava/lang/IllegalArgumentException;-><init>(Ljava/lang/String;)V

    throw v1

    .line 304
    :cond_8
    iput-boolean v6, v1, La/a/a/a/KC_e;->p:Z

    .line 308
    iget-boolean v3, v1, La/a/a/a/KC_e;->C:Z

    if-nez v3, :cond_9

    .line 309
    iget-object v3, v1, La/a/a/a/KC_e;->d:Landroid/os/Bundle;

    invoke-virtual {v1}, La/a/a/a/KC_e;->k()V

    .line 311
    :cond_9
    iget-object v3, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v3, v1}, La/a/a/a/KC_i;->b(La/a/a/a/KC_e;)V

    goto :goto_2

    .line 318
    :cond_a
    if-eqz v2, :cond_b

    .line 319
    iget-object v0, v1, La/a/a/a/KC_e;->H:Landroid/view/View;

    invoke-virtual {v0, v2}, Landroid/view/View;->setId(I)V

    .line 321
    :cond_b
    iget-object v0, v1, La/a/a/a/KC_e;->H:Landroid/view/View;

    invoke-virtual {v0}, Landroid/view/View;->getTag()Ljava/lang/Object;

    move-result-object v0

    if-nez v0, :cond_c

    .line 322
    iget-object v0, v1, La/a/a/a/KC_e;->H:Landroid/view/View;

    invoke-virtual {v0, v5}, Landroid/view/View;->setTag(Ljava/lang/Object;)V

    .line 324
    :cond_c
    iget-object v0, v1, La/a/a/a/KC_e;->H:Landroid/view/View;

    goto/16 :goto_0
.end method

.method protected onDestroy()V
    .locals 1

    .prologue
    .line 332
    invoke-super {p0}, Landroid/app/Activity;->onDestroy()V

    .line 334
    const/4 v0, 0x0

    invoke-virtual {p0, v0}, La/a/a/a/KC_f;->a(Z)V

    .line 336
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->p()V

    .line 337
    iget-object v0, p0, La/a/a/a/KC_f;->m:La/a/a/a/KC_o;

    if-eqz v0, :cond_0

    .line 338
    iget-object v0, p0, La/a/a/a/KC_f;->m:La/a/a/a/KC_o;

    invoke-virtual {v0}, La/a/a/a/KC_o;->h()V

    .line 340
    :cond_0
    return-void
.end method

.method public onKeyDown(ILandroid/view/KeyEvent;)Z
    .locals 2
    .param p1, "keyCode"    # I
    .param p2, "event"    # Landroid/view/KeyEvent;

    .prologue
    .line 347
    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/4 v1, 0x5

    if-ge v0, v1, :cond_0

    const/4 v0, 0x4

    if-ne p1, v0, :cond_0

    invoke-virtual {p2}, Landroid/view/KeyEvent;->getRepeatCount()I

    move-result v0

    if-nez v0, :cond_0

    .line 352
    invoke-virtual {p0}, La/a/a/a/KC_f;->onBackPressed()V

    .line 353
    const/4 v0, 0x1

    .line 356
    :goto_0
    return v0

    :cond_0
    invoke-super {p0, p1, p2}, Landroid/app/Activity;->onKeyDown(ILandroid/view/KeyEvent;)Z

    move-result v0

    goto :goto_0
.end method

.method public onLowMemory()V
    .locals 1

    .prologue
    .line 364
    invoke-super {p0}, Landroid/app/Activity;->onLowMemory()V

    .line 365
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->q()V

    .line 366
    return-void
.end method

.method public onMenuItemSelected(ILandroid/view/MenuItem;)Z
    .locals 1
    .param p1, "featureId"    # I
    .param p2, "item"    # Landroid/view/MenuItem;

    .prologue
    .line 373
    invoke-super {p0, p1, p2}, Landroid/app/Activity;->onMenuItemSelected(ILandroid/view/MenuItem;)Z

    move-result v0

    if-eqz v0, :cond_0

    .line 374
    const/4 v0, 0x1

    .line 385
    :goto_0
    return v0

    .line 377
    :cond_0
    sparse-switch p1, :sswitch_data_0

    .line 385
    const/4 v0, 0x0

    goto :goto_0

    .line 379
    :sswitch_0
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0, p2}, La/a/a/a/KC_i;->a(Landroid/view/MenuItem;)Z

    move-result v0

    goto :goto_0

    .line 382
    :sswitch_1
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0, p2}, La/a/a/a/KC_i;->b(Landroid/view/MenuItem;)Z

    move-result v0

    goto :goto_0

    .line 377
    nop

    :sswitch_data_0
    .sparse-switch
        0x0 -> :sswitch_0
        0x6 -> :sswitch_1
    .end sparse-switch
.end method

.method protected onNewIntent(Landroid/content/Intent;)V
    .locals 1
    .param p1, "intent"    # Landroid/content/Intent;

    .prologue
    .line 428
    invoke-super {p0, p1}, Landroid/app/Activity;->onNewIntent(Landroid/content/Intent;)V

    .line 429
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->g()V

    .line 430
    return-void
.end method

.method public onPanelClosed(ILandroid/view/Menu;)V
    .locals 1
    .param p1, "featureId"    # I
    .param p2, "menu"    # Landroid/view/Menu;

    .prologue
    .line 394
    packed-switch p1, :pswitch_data_0

    .line 399
    :goto_0
    invoke-super {p0, p1, p2}, Landroid/app/Activity;->onPanelClosed(ILandroid/view/Menu;)V

    .line 400
    return-void

    .line 396
    :pswitch_0
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0, p2}, La/a/a/a/KC_i;->b(Landroid/view/Menu;)V

    goto :goto_0

    .line 394
    nop

    :pswitch_data_0
    .packed-switch 0x0
        :pswitch_0
    .end packed-switch
.end method

.method protected onPause()V
    .locals 2

    .prologue
    const/4 v1, 0x2

    .line 407
    invoke-super {p0}, Landroid/app/Activity;->onPause()V

    .line 408
    const/4 v0, 0x0

    iput-boolean v0, p0, La/a/a/a/KC_f;->g:Z

    .line 409
    iget-object v0, p0, La/a/a/a/KC_f;->a:Landroid/os/Handler;

    invoke-virtual {v0, v1}, Landroid/os/Handler;->hasMessages(I)Z

    move-result v0

    if-eqz v0, :cond_0

    .line 410
    iget-object v0, p0, La/a/a/a/KC_f;->a:Landroid/os/Handler;

    invoke-virtual {v0, v1}, Landroid/os/Handler;->removeMessages(I)V

    .line 411
    invoke-virtual {p0}, La/a/a/a/KC_f;->onResumeFragments()V

    .line 413
    :cond_0
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->l()V

    .line 414
    return-void
.end method

.method protected onPostResume()V
    .locals 2

    .prologue
    .line 454
    invoke-super {p0}, Landroid/app/Activity;->onPostResume()V

    .line 455
    iget-object v0, p0, La/a/a/a/KC_f;->a:Landroid/os/Handler;

    const/4 v1, 0x2

    invoke-virtual {v0, v1}, Landroid/os/Handler;->removeMessages(I)V

    .line 456
    invoke-virtual {p0}, La/a/a/a/KC_f;->onResumeFragments()V

    .line 457
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->d()Z

    .line 458
    return-void
.end method

.method protected onPrepareOptionsPanel(Landroid/view/View;Landroid/view/Menu;)Z
    .locals 1
    .param p1, "view"    # Landroid/view/View;
    .param p2, "menu"    # Landroid/view/Menu;

    .prologue
    .line 492
    const/4 v0, 0x0

    invoke-super {p0, v0, p1, p2}, Landroid/app/Activity;->onPreparePanel(ILandroid/view/View;Landroid/view/Menu;)Z

    move-result v0

    return v0
.end method

.method public onPreparePanel(ILandroid/view/View;Landroid/view/Menu;)Z
    .locals 2
    .param p1, "featureId"    # I
    .param p2, "view"    # Landroid/view/View;
    .param p3, "menu"    # Landroid/view/Menu;

    .prologue
    .line 475
    if-nez p1, :cond_1

    if-eqz p3, :cond_1

    .line 476
    iget-boolean v0, p0, La/a/a/a/KC_f;->i:Z

    if-eqz v0, :cond_0

    .line 477
    const/4 v0, 0x0

    iput-boolean v0, p0, La/a/a/a/KC_f;->i:Z

    .line 478
    invoke-interface {p3}, Landroid/view/Menu;->clear()V

    .line 479
    invoke-virtual {p0, p1, p3}, La/a/a/a/KC_f;->onCreatePanelMenu(ILandroid/view/Menu;)Z

    .line 481
    :cond_0
    invoke-virtual {p0, p2, p3}, La/a/a/a/KC_f;->onPrepareOptionsPanel(Landroid/view/View;Landroid/view/Menu;)Z

    move-result v0

    .line 482
    iget-object v1, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v1, p3}, La/a/a/a/KC_i;->a(Landroid/view/Menu;)Z

    move-result v1

    or-int/2addr v0, v1

    .line 485
    :goto_0
    return v0

    :cond_1
    invoke-super {p0, p1, p2, p3}, Landroid/app/Activity;->onPreparePanel(ILandroid/view/View;Landroid/view/Menu;)Z

    move-result v0

    goto :goto_0
.end method

.method protected onResume()V
    .locals 2

    .prologue
    .line 443
    invoke-super {p0}, Landroid/app/Activity;->onResume()V

    .line 444
    iget-object v0, p0, La/a/a/a/KC_f;->a:Landroid/os/Handler;

    const/4 v1, 0x2

    invoke-virtual {v0, v1}, Landroid/os/Handler;->sendEmptyMessage(I)Z

    .line 445
    const/4 v0, 0x1

    iput-boolean v0, p0, La/a/a/a/KC_f;->g:Z

    .line 446
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->d()Z

    .line 447
    return-void
.end method

.method protected onResumeFragments()V
    .locals 1

    .prologue
    .line 467
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->k()V

    .line 468
    return-void
.end method

.method public onRetainCustomNonConfigurationInstance()Ljava/lang/Object;
    .locals 1

    .prologue
    .line 625
    const/4 v0, 0x0

    return-object v0
.end method

.method public final onRetainNonConfigurationInstance()Ljava/lang/Object;
    .locals 9

    .prologue
    const/4 v1, 0x1

    const/4 v2, 0x0

    .line 502
    iget-boolean v0, p0, La/a/a/a/KC_f;->c:Z

    if-eqz v0, :cond_0

    .line 503
    invoke-virtual {p0, v1}, La/a/a/a/KC_f;->a(Z)V

    .line 506
    :cond_0
    invoke-virtual {p0}, La/a/a/a/KC_f;->onRetainCustomNonConfigurationInstance()Ljava/lang/Object;

    move-result-object v4

    .line 508
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->e()Ljava/util/ArrayList;

    move-result-object v5

    .line 510
    iget-object v0, p0, La/a/a/a/KC_f;->l:La/a/a/d/KC_e;

    if-eqz v0, :cond_3

    .line 513
    iget-object v0, p0, La/a/a/a/KC_f;->l:La/a/a/d/KC_e;

    invoke-virtual {v0}, La/a/a/d/KC_e;->size()I

    move-result v6

    .line 514
    new-array v7, v6, [La/a/a/a/KC_o;

    .line 515
    add-int/lit8 v0, v6, -0x1

    move v3, v0

    :goto_0
    if-ltz v3, :cond_1

    .line 516
    iget-object v0, p0, La/a/a/a/KC_f;->l:La/a/a/d/KC_e;

    invoke-virtual {v0, v3}, La/a/a/d/KC_e;->b(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/a/KC_o;

    aput-object v0, v7, v3

    .line 515
    add-int/lit8 v0, v3, -0x1

    move v3, v0

    goto :goto_0

    :cond_1
    move v0, v2

    .line 518
    :goto_1
    if-ge v2, v6, :cond_4

    .line 519
    aget-object v3, v7, v2

    .line 520
    iget-boolean v8, v3, La/a/a/a/KC_o;->e:Z

    if-eqz v8, :cond_2

    move v0, v1

    .line 518
    :goto_2
    add-int/lit8 v2, v2, 0x1

    goto :goto_1

    .line 523
    :cond_2
    invoke-virtual {v3}, La/a/a/a/KC_o;->h()V

    .line 524
    iget-object v8, p0, La/a/a/a/KC_f;->l:La/a/a/d/KC_e;

    iget-object v3, v3, La/a/a/a/KC_o;->b:Ljava/lang/String;

    invoke-virtual {v8, v3}, La/a/a/d/KC_e;->remove(Ljava/lang/Object;)Ljava/lang/Object;

    goto :goto_2

    :cond_3
    move v0, v2

    .line 528
    :cond_4
    if-nez v5, :cond_5

    if-nez v0, :cond_5

    if-nez v4, :cond_5

    .line 529
    const/4 v0, 0x0

    .line 538
    :goto_3
    return-object v0

    .line 532
    :cond_5
    new-instance v0, La/a/a/a/KC_f$KC_b;

    invoke-direct {v0}, La/a/a/a/KC_f$KC_b;-><init>()V

    .line 533
    iput-object v4, v0, La/a/a/a/KC_f$KC_b;->a:Ljava/lang/Object;

    .line 535
    iput-object v5, v0, La/a/a/a/KC_f$KC_b;->b:Ljava/util/ArrayList;

    .line 537
    iget-object v1, p0, La/a/a/a/KC_f;->l:La/a/a/d/KC_e;

    iput-object v1, v0, La/a/a/a/KC_f$KC_b;->c:La/a/a/d/KC_e;

    goto :goto_3
.end method

.method protected onSaveInstanceState(Landroid/os/Bundle;)V
    .locals 2
    .param p1, "outState"    # Landroid/os/Bundle;

    .prologue
    .line 546
    invoke-super {p0, p1}, Landroid/app/Activity;->onSaveInstanceState(Landroid/os/Bundle;)V

    .line 547
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->f()Landroid/os/Parcelable;

    move-result-object v0

    .line 548
    if-eqz v0, :cond_0

    .line 549
    const-string v1, "android:support:fragments"

    invoke-virtual {p1, v1, v0}, Landroid/os/Bundle;->putParcelable(Ljava/lang/String;Landroid/os/Parcelable;)V

    .line 551
    :cond_0
    return-void
.end method

.method protected onStart()V
    .locals 5

    .prologue
    const/4 v3, 0x1

    const/4 v1, 0x0

    .line 559
    invoke-super {p0}, Landroid/app/Activity;->onStart()V

    .line 561
    iput-boolean v1, p0, La/a/a/a/KC_f;->c:Z

    .line 562
    iput-boolean v1, p0, La/a/a/a/KC_f;->h:Z

    .line 563
    iget-object v0, p0, La/a/a/a/KC_f;->a:Landroid/os/Handler;

    invoke-virtual {v0, v3}, Landroid/os/Handler;->removeMessages(I)V

    .line 565
    iget-boolean v0, p0, La/a/a/a/KC_f;->f:Z

    if-nez v0, :cond_0

    .line 566
    iput-boolean v3, p0, La/a/a/a/KC_f;->f:Z

    .line 567
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->i()V

    .line 570
    :cond_0
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->g()V

    .line 571
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->d()Z

    .line 573
    iget-boolean v0, p0, La/a/a/a/KC_f;->k:Z

    if-nez v0, :cond_2

    .line 574
    iput-boolean v3, p0, La/a/a/a/KC_f;->k:Z

    .line 575
    iget-object v0, p0, La/a/a/a/KC_f;->m:La/a/a/a/KC_o;

    if-eqz v0, :cond_3

    .line 576
    iget-object v0, p0, La/a/a/a/KC_f;->m:La/a/a/a/KC_o;

    invoke-virtual {v0}, La/a/a/a/KC_o;->b()V

    .line 584
    :cond_1
    :goto_0
    iput-boolean v3, p0, La/a/a/a/KC_f;->j:Z

    .line 588
    :cond_2
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->j()V

    .line 589
    iget-object v0, p0, La/a/a/a/KC_f;->l:La/a/a/d/KC_e;

    if-eqz v0, :cond_5

    .line 590
    iget-object v0, p0, La/a/a/a/KC_f;->l:La/a/a/d/KC_e;

    invoke-virtual {v0}, La/a/a/d/KC_e;->size()I

    move-result v3

    .line 591
    new-array v4, v3, [La/a/a/a/KC_o;

    .line 592
    add-int/lit8 v0, v3, -0x1

    move v2, v0

    :goto_1
    if-ltz v2, :cond_4

    .line 593
    iget-object v0, p0, La/a/a/a/KC_f;->l:La/a/a/d/KC_e;

    invoke-virtual {v0, v2}, La/a/a/d/KC_e;->b(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/a/KC_o;

    aput-object v0, v4, v2

    .line 592
    add-int/lit8 v0, v2, -0x1

    move v2, v0

    goto :goto_1

    .line 577
    :cond_3
    iget-boolean v0, p0, La/a/a/a/KC_f;->j:Z

    if-nez v0, :cond_1

    .line 578
    const-string v0, "(root)"

    iget-boolean v2, p0, La/a/a/a/KC_f;->k:Z

    invoke-virtual {p0, v0, v2, v1}, La/a/a/a/KC_f;->a(Ljava/lang/String;ZZ)La/a/a/a/KC_o;

    move-result-object v0

    iput-object v0, p0, La/a/a/a/KC_f;->m:La/a/a/a/KC_o;

    .line 580
    iget-object v0, p0, La/a/a/a/KC_f;->m:La/a/a/a/KC_o;

    if-eqz v0, :cond_1

    iget-object v0, p0, La/a/a/a/KC_f;->m:La/a/a/a/KC_o;

    iget-boolean v0, v0, La/a/a/a/KC_o;->d:Z

    if-nez v0, :cond_1

    .line 581
    iget-object v0, p0, La/a/a/a/KC_f;->m:La/a/a/a/KC_o;

    invoke-virtual {v0}, La/a/a/a/KC_o;->b()V

    goto :goto_0

    :cond_4
    move v0, v1

    .line 595
    :goto_2
    if-ge v0, v3, :cond_5

    .line 596
    aget-object v1, v4, v0

    .line 597
    invoke-virtual {v1}, La/a/a/a/KC_o;->e()V

    .line 598
    invoke-virtual {v1}, La/a/a/a/KC_o;->g()V

    .line 595
    add-int/lit8 v0, v0, 0x1

    goto :goto_2

    .line 601
    :cond_5
    return-void
.end method

.method protected onStop()V
    .locals 2

    .prologue
    const/4 v1, 0x1

    .line 608
    invoke-super {p0}, Landroid/app/Activity;->onStop()V

    .line 610
    iput-boolean v1, p0, La/a/a/a/KC_f;->c:Z

    .line 611
    iget-object v0, p0, La/a/a/a/KC_f;->a:Landroid/os/Handler;

    invoke-virtual {v0, v1}, Landroid/os/Handler;->sendEmptyMessage(I)Z

    .line 613
    iget-object v0, p0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->m()V

    .line 614
    return-void
.end method

.method public startActivityForResult(Landroid/content/Intent;I)V
    .locals 2
    .param p1, "intent"    # Landroid/content/Intent;
    .param p2, "requestCode"    # I

    .prologue
    .line 837
    const/4 v0, -0x1

    if-eq p2, v0, :cond_0

    const/high16 v0, -0x10000

    and-int/2addr v0, p2

    if-eqz v0, :cond_0

    .line 838
    new-instance v0, Ljava/lang/IllegalArgumentException;

    const-string v1, "Can only use lower 16 bits for requestCode"

    invoke-direct {v0, v1}, Ljava/lang/IllegalArgumentException;-><init>(Ljava/lang/String;)V

    throw v0

    .line 840
    :cond_0
    invoke-super {p0, p1, p2}, Landroid/app/Activity;->startActivityForResult(Landroid/content/Intent;I)V

    .line 841
    return-void
.end method

.method public startActivityFromFragment(La/a/a/a/KC_e;Landroid/content/Intent;I)V
    .locals 2
    .param p1, "fragment"    # La/a/a/a/KC_e;
    .param p2, "intent"    # Landroid/content/Intent;
    .param p3, "requestCode"    # I

    .prologue
    const/4 v0, -0x1

    .line 848
    if-ne p3, v0, :cond_0

    .line 849
    invoke-super {p0, p2, v0}, Landroid/app/Activity;->startActivityForResult(Landroid/content/Intent;I)V

    .line 856
    :goto_0
    return-void

    .line 852
    :cond_0
    const/high16 v0, -0x10000

    and-int/2addr v0, p3

    if-eqz v0, :cond_1

    .line 853
    new-instance v0, Ljava/lang/IllegalArgumentException;

    const-string v1, "Can only use lower 16 bits for requestCode"

    invoke-direct {v0, v1}, Ljava/lang/IllegalArgumentException;-><init>(Ljava/lang/String;)V

    throw v0

    .line 855
    :cond_1
    iget v0, p1, La/a/a/a/KC_e;->f:I

    add-int/lit8 v0, v0, 0x1

    shl-int/lit8 v0, v0, 0x10

    const v1, 0xffff

    and-int/2addr v1, p3

    add-int/2addr v0, v1

    invoke-super {p0, p2, v0}, Landroid/app/Activity;->startActivityForResult(Landroid/content/Intent;I)V

    goto :goto_0
.end method

.method public supportInvalidateOptionsMenu()V
    .locals 2

    .prologue
    .line 646
    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v1, 0xb

    if-lt v0, v1, :cond_0

    .line 649
    invoke-virtual {p0}, Landroid/app/Activity;->invalidateOptionsMenu()V

    .line 656
    :goto_0
    return-void

    .line 655
    :cond_0
    const/4 v0, 0x1

    iput-boolean v0, p0, La/a/a/a/KC_f;->i:Z

    goto :goto_0
.end method
